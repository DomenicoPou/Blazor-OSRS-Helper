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
using System.Xml.Linq;
using Blazor_OSRS_Helper.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OSRS_DataWorker.Handlers;

namespace OSRS_DataWorker
{
    public class ShopWorker : BackgroundService
    {
        private readonly ILogger<ShopWorker> _logger;
        private static Dictionary<string, List<int>> itemLookup;
        private static string shopFile = Path.Combine(ApplicationConfigHandler.config.DownloadLocation, "OSRSShopList.json");

        public ShopWorker(ILogger<ShopWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Shop Worker running at: {time}", DateTimeOffset.Now);
                HtmlDocument doc = ObtainHtml(@"https://oldschool.runescape.wiki/w/Shop");
                List<HtmlNode> nodes = doc.DocumentNode.Descendants("li").ToList();

                itemLookup = ApplicationConfigHandler.GetCurrentItemLookup();
                if (itemLookup.Count > 0)
                {
                    Dictionary<string, RSShop> stores = new Dictionary<string, RSShop>();
                    int max = nodes.Count;
                    int count = 0;
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        try
                        {

                            List<HtmlNode> hrefNodes = nodes[i].Descendants("a").Where(node => node.InnerText != "").ToList();
                            foreach (HtmlNode storeNode in hrefNodes)
                            {
                                if (!stores.ContainsKey(storeNode.InnerText))
                                    stores.Add(storeNode.InnerText, ObtainStoreModel(storeNode.GetAttributeValue("href", "")));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (nodes[i].InnerText.Contains("Shops")) break;
                        }
                        count++;
                        _logger.LogInformation($"Shop Worker progress {count} / {max} (estimated)");
                    }

                    if (stores.Count > 0)
                    {
                        // Write the string into the config file
                        using (StreamWriter writer = new StreamWriter(shopFile, false, Encoding.Unicode))
                        {
                            writer.Write(JsonConvert.SerializeObject(stores.Values, Formatting.Indented));
                        }
                    }
                    await Task.Delay((1000 * 60 * 60 * 24), stoppingToken); // Wait 24 hrs
                }
                else
                {
                    _logger.LogInformation("Shop Worker has no item lookup");
                    await Task.Delay(1000, stoppingToken); // Wait 1 second
                }
            }
        }

        private RSShop ObtainStoreModel(string url)
        {
            RSShop newShop = new RSShop();
            HtmlDocument doc = ObtainHtml($@"https://oldschool.runescape.wiki{url}");


            HtmlNode metaTableNode = doc.DocumentNode.Descendants("table").ToList()[0];
            List<HtmlNode> headers = metaTableNode.Descendants("th").ToList();

            // Obtain Name
            newShop.name = headers[0].InnerText.ToLower().Trim();
            
            // Obtain is member
            newShop.isMember = (headers[2].NextSibling.InnerText.ToLower() == "yes");

            // Obtain Location
            newShop.location = headers[4].NextSibling.InnerText.ToLower().Trim();

            // Obtain Owner
            newShop.owner = headers[5].NextSibling.InnerText.ToLower().Trim();

            // Obtain Specialty
            newShop.specialty = headers[6].NextSibling.InnerText.ToLower().Trim();

            // Obtain Stock
            List<HtmlNode> stockTablNode = doc.DocumentNode.Descendants("h2").Where(x => x.InnerText.Contains("Stock")).ToList();

            if (stockTablNode.Count > 0)
            {
                newShop.stock = ObtainStock(stockTablNode[0].NextSibling.NextSibling);
            } else
            {
                newShop.stock = new List<RSStock>();
            }

            return newShop;
        }

        private List<RSStock> ObtainStock(HtmlNode node)
        {
            List<RSStock> newStockList = new List<RSStock>();
            List<HtmlNode> itemList = node.Descendants("tr").ToList();
            for (int i = 1; i < itemList.Count; i++)
            {
                List<HtmlNode> columnList = itemList[i].Descendants("td").ToList();
                if (columnList.Count > 0)
                {
                    string key = columnList[1].InnerText.ToLower();
                    if (itemLookup.ContainsKey(key))
                    {
                        RSStock newStock = new RSStock();
                        newStock.isNotCoin = false;
                        newStock.itemId = itemLookup[key][0];
                        newStock.numberInStock = ObtainInt(columnList[2].InnerText);
                        newStock.isInfinite = (columnList[2].InnerText == "∞");
                        newStock.priceSoldAt = ObtainInt(columnList[3].InnerText);

                        if (columnList.Count == 6)
                        {
                            newStock.priceBoughtAt = ObtainInt(columnList[4].InnerText);
                        }

                        if (columnList.Count == 7)
                        {
                            newStock.isNotCoin = true;
                            newStock.priceBoughtAt = ObtainInt(columnList[4].InnerText);
                        }

                        newStockList.Add(newStock);
                    }
                }
                else
                {

                }
            }
            return newStockList;
        }

        private int? ObtainInt(string innerText)
        {
            if (int.TryParse(innerText, out int value))
            {
                return value;
            } else
            {
                return null;
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
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("429"))
                {
                    Thread.Sleep(5000);
                    return ObtainHtml(url);
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
