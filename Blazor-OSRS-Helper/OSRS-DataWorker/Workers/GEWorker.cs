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
using OSRS_DataWorker.Models;

namespace OSRS_DataWorker
{
    public class GEWorker : BackgroundService
    {
        private readonly ILogger<ItemWorker> _logger;
        private static Dictionary<string, List<int>> itemLookup;
        private static string geFile = Path.Combine(ApplicationConfigHandler.config.DownloadLocation, "OSRSGEList.json");

        public GEWorker(ILogger<ItemWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("GE Worker running at: {time}", DateTimeOffset.Now);
                List<RSItem> items = ApplicationConfigHandler.GetCurrentItems().Where(x => x.tradeableOnGE == true).ToList();
                List<RSGETrend> trends = new List<RSGETrend>();
                int count = 0;
                int maxCount = items.Count;
                List<Task> tasks = items.Select(async item =>
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync($"https://services.runescape.com/m=itemdb_rs/api/catalogue/detail.json?item={item.id}");
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (responseBody != "")
                        {
                            try
                            {
                                ReturnedTrend returnedItemTrend = JsonConvert.DeserializeObject<ReturnedTrend>(responseBody);

                                RSGETrend trend = new RSGETrend();
                                trend.itemId = item.id;
                                trend.current = 0;

                                if (int.TryParse(returnedItemTrend.item.current.price.Replace(",", string.Empty), out int val))
                                    trend.current = val;

                                trend.changes = new Dictionary<int, float?>();
                                trend.changes.Add(30, ObtainPercentageAsFloat(returnedItemTrend.item.day30.change));
                                trend.changes.Add(90, ObtainPercentageAsFloat(returnedItemTrend.item.day90.change));
                                trend.changes.Add(180, ObtainPercentageAsFloat(returnedItemTrend.item.day180.change));

                                trends.Add(trend);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    count++;
                    _logger.LogInformation("GE Worker Count {count}: {maxCount}", count, maxCount);

                }).ToList();

                if (trends.Count > 0)
                {
                    // Write the string into the config file
                    using (StreamWriter writer = new StreamWriter(geFile, false, Encoding.Unicode))
                    {
                        writer.Write(JsonConvert.SerializeObject(trends, Formatting.Indented));
                    }
                }
                await Task.WhenAll(tasks);
                await Task.Delay((1000 * 60 * 60 * 24), stoppingToken); // Wait 24 hrs
            }
        }

        private float? ObtainPercentageAsFloat(string change)
        {
            if (change.Contains("+"))
            {
                return float.Parse(change.Substring(1, change.Length - 2));
            } else if (change.Contains("-"))
            {

                return -1 * float.Parse(change.Substring(1, change.Length - 2));
            } if (change == "0.0%")
            {
                return 0;
            }
            return null;
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
    }
}
