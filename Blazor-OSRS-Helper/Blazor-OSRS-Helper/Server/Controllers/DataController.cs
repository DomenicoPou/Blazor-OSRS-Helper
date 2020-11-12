using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazor_OSRS_Helper.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Blazor_OSRS_Helper.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        public string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        [HttpGet("Item")]
        public List<RSItem> GetItems()
        {
            // Deserialize Json to Object and Return It
            using (var fileStream = new FileStream(Path.Combine(ConfigFolder, "OSRSItemList.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var textReader = new StreamReader(fileStream))
            {
                string content = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject< List<RSItem>>(content);
            }
        }

        [HttpGet("Shop")]
        public List<RSShop> GetShops()
        {
            // Deserialize Json to Object and Return It
            using (var fileStream = new FileStream(Path.Combine(ConfigFolder, "OSRSShopList.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var textReader = new StreamReader(fileStream))
            {
                string content = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<RSShop>>(content);
            }
        }

        [HttpGet("GEData")]
        public List<RSGETrend> GetGEData()
        {
            // Deserialize Json to Object and Return It
            using (var fileStream = new FileStream(Path.Combine(ConfigFolder, "OSRSGEList.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var textReader = new StreamReader(fileStream))
            {
                string content = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject< List<RSGETrend>>(content);
            }
        }
    }
}
