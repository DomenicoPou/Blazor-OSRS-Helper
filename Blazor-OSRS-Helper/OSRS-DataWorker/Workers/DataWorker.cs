using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazor_OSRS_Helper.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OSRS_DataWorker
{
    public class DataWorker : BackgroundService
    {
        private readonly ILogger<DataWorker> _logger;
        private static Dictionary<string, List<int>> itemLookup;
        public DataWorker(ILogger<DataWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString(@"https://www.osrsbox.com/osrsbox-db/items-complete.json");
                    List<RSItem> items = JsonConvert.DeserializeObject<Dictionary<string, RSItem>>(json)
                        .Values.Where(item => item.duplicate == false).ToList();
                    itemLookup = items.GroupBy(item => item.name.ToLower().Trim())
                        .ToDictionary(key => key.Key, value => value.Select(i => i.id).ToList());
                    Parallel.ForEach(items, (item) => {
                        try
                        {
                            if (item.wikiUrl != null && item.wikiUrl != "")
                            {
                                HtmlDocument itemHtmlDoc = ObtainHtml(item.wikiUrl);
                                List<HtmlNode> descriptionNodes = itemHtmlDoc.DocumentNode.Descendants("meta").Where(node => node.GetAttributeValue("name", "")
                                    .Contains("description")).ToList();
                                if (descriptionNodes.Count() > 0) item.description = descriptionNodes[0].GetAttributeValue("content", "");
                                List<HtmlNode> creationContainers = itemHtmlDoc.DocumentNode.Descendants("h2").Where(node => node.InnerHtml.Contains("Creation")).ToList();
                                item.isCraftable = false;
                                if (creationContainers.Count() > 0)
                                {
                                    item.isCraftable = true;
                                    HtmlNode creationNode = creationContainers[0].NextSibling.NextSibling;
                                    item.craftingMethods = ObtainCraftingMethods(creationNode, item.id);
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Error: Item {id} ({name}) no url {url}", item.id, item.name, item.wikiUrl);
                            }
                        } catch (Exception ex)
                        {

                        }
                    });


                    // Write the string into the config file
                    using (StreamWriter writer = new StreamWriter($@"AppDomain.CurrentDomain.BaseDirectory\OSRSItemList.json", false, Encoding.Unicode))
                    {
                        writer.Write(JsonConvert.SerializeObject(items, Formatting.Indented)); ;
                    }

                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private static HtmlDocument ObtainHtml(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                string htmlString = client.GetStringAsync(url).Result;
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(htmlString);
                return html;
            } catch (Exception ex)
            {
                if (ex.Message.Contains("429"))
                {
                    Thread.Sleep(5000);
                    return ObtainHtml(url);
                } else
                {
                    throw ex;
                }
            }
        }

        public static List<RSCraftingMethod> ObtainCraftingMethods(HtmlNode creationNode, int itemId)
        {
            List<RSCraftingMethod> methods = new List<RSCraftingMethod>();

            List<HtmlNode> requirmentTableNodes = creationNode.Descendants("table").Where(node => node.InnerHtml.Contains("Requirements")).ToList();
            List<HtmlNode> materialTableNodes = creationNode.Descendants("table").Where(node => node.InnerHtml.Contains("Materials") && !node.InnerHtml.Contains("Skills")).ToList();

            for (int i = 0; i < requirmentTableNodes.Count(); i++)
            {
                RSCraftingMethod method = new RSCraftingMethod();
                List<HtmlNode> requirmentNodes = requirmentTableNodes[i].Descendants("tr").ToList();
                method.skillRequirments = GetSkillRequirments(requirmentNodes, 
                    out bool? isMember,
                    out List<int> tools,
                    out List<string> facilities,
                    out int ticks,
                    out decimal experienceGained);
                method.isMember = isMember ?? default;
                method.tools = tools;
                method.facilities = facilities;
                method.ticks = ticks;
                method.experienceGained = experienceGained;

                List<HtmlNode> requirmentmaterialNodes = materialTableNodes[i].Descendants("tr").ToList();
                method.materials = GetMaterialsRequired(requirmentmaterialNodes, itemId, out int quantityMade);
                method.quantityMade = quantityMade;
                methods.Add(method);
            }
            return methods;
        }

        private static Dictionary<string, int> GetSkillRequirments(
            List<HtmlNode> requirmentNodes, 
            out bool? isMember,
            out List<int> tools,
            out List<string> facilities,
            out int ticks,
            out decimal experienceGained)
        {
            isMember = null;
            tools = new List<int>();
            facilities = new List<string>();
            ticks = 0;
            experienceGained = 0;

            Dictionary<string, int> requirments = new Dictionary<string, int>();
            foreach (HtmlNode requirmentNode in requirmentNodes)
            {
                // Get requirments
                List<HtmlNode> requirmentRows = requirmentNode.Descendants("td").ToList();
                if (requirmentRows.Count == 3) // Skill | Level | Experience
                {
                    requirments.Add(
                        requirmentRows[0].InnerText.Trim().ToLower(), 
                        Convert.ToInt32(requirmentRows[1].InnerHtml.Split('<')[0].Trim())
                        );
                    experienceGained = Convert.ToDecimal(requirmentRows[2].InnerHtml);
                }
                if (requirmentRows.Count == 2)
                {
                    // Check status of member
                    if (isMember == null)
                    {
                        if (requirmentRows[0].InnerHtml.Contains("Member_icon.png"))
                        {
                            isMember = true;
                        }
                        else if (requirmentRows[0].InnerHtml.Contains("Free-to-play_icon.png"))
                        {
                            isMember = false;
                        }

                        string tickString = requirmentRows[1].InnerHtml.Split("(")[0].Trim();
                        if (int.TryParse(tickString, out int tickResult))
                        {
                            ticks = tickResult;
                        }

                    } else if (isMember != null)
                    {
                        // Get tools
                        List<HtmlNode> toolNodes = requirmentRows[0].Descendants("a").ToList();
                        tools = GetIconNamesIds(requirmentRows[0]);
                        facilities = GetIconNames(requirmentRows[1]);
                    }
                }
            }
            return requirments;
        }


        private static List<int> GetIconNamesIds(HtmlNode htmlNode)
        {
            List<int> ids = new List<int>();
            foreach (HtmlNode node in htmlNode.Descendants("a").Where(node => node.InnerHtml.Contains("img")))
            {
                string name = node.GetAttributeValue("title", "").Trim().ToLower();
                ids.Add(itemLookup[name][0]);
            }
            return ids;
        }


        private static List<string> GetIconNames(HtmlNode htmlNode)
        {
            List<string> names = new List<string>();
            foreach (HtmlNode node in htmlNode.Descendants("a").Where(node => node.InnerHtml.Contains("img")))
            {
                string name = node.GetAttributeValue("title", "").Trim().ToLower();
                names.Add(name);
            }
            return names;
        }


        private static Dictionary<int, int> GetMaterialsRequired(List<HtmlNode> requirmentmaterialNodes, int itemId, out int quantityMade)
        {
            Dictionary<int, int> materialRequired = new Dictionary<int, int>();
            quantityMade = 0; 
            foreach (HtmlNode materialNode in requirmentmaterialNodes)
            {
                List<HtmlNode> materialRows = materialNode.Descendants("td").ToList();
                if (materialRows.Count == 4)
                {
                    int quantity = Convert.ToInt32(materialRows[2].InnerText.Trim().ToLower());
                    int materialID = itemLookup[materialRows[1].InnerText.Trim().ToLower()][0];
                    if (itemId == materialID)
                    {
                        quantityMade = quantity;
                    } else
                    {
                        materialRequired.Add(materialID, quantity);
                    }
                }
                quantityMade = 0;
            }
            return materialRequired;
        }
    }
}
