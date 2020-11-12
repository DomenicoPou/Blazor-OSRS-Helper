using Blazor_OSRS_Helper.Shared.Models;
using Newtonsoft.Json;
using OSRS_DataWorker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OSRS_DataWorker.Handlers
{
    public static class ApplicationConfigHandler
    {
        public static ApplicationConfigModel config;

        static ApplicationConfigHandler()
        {
            config = ConfigurationHandler<ApplicationConfigModel>.ReadConfig();
        }

        public static Dictionary<string, List<int>> GetCurrentItemLookup()
        {
            return ObtainCurrentItems<RSItem>(Path.Combine(config.DownloadLocation, "OSRSItemList.json")).GroupBy(item => item.name.ToLower().Trim())
                        .ToDictionary(key => key.Key, value => value.Select(i => i.id).ToList());
        }

        public static List<RSItem> GetCurrentItems()
        {
            return ObtainCurrentItems<RSItem>(Path.Combine(config.DownloadLocation, "OSRSItemList.json")).ToList();
        }

        private static List<T> ObtainCurrentItems<T>(string file)
        {
            if (File.Exists(file))
            {
                // Deserialize Json to Object and Return It
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var textReader = new StreamReader(fileStream))
                {
                    string content = textReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<T>>(content);
                }
            }
            else
            {
                return new List<T>();
            }
        }
    }
}
