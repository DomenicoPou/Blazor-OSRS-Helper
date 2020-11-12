using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OSRS_DataWorker.Handlers
{
    /// <summary>
    /// This configuration handler implements and handles a configuration folder with json files
    /// </summary>
    /// <typeparam name="T">The type of configuration being handled</typeparam>
    public static class ConfigurationHandler<T>
    {
        // T will be the main configuartion class
        private static string configurationFolderName = "Config";

        /// <summary>
        /// Initial constructure  
        /// </summary>
        static ConfigurationHandler()
        {
            SetupConfiguration();
        }

        /// <summary>
        /// Writes the base configuration model if the file doesn't exist
        /// </summary>
        private static void SetupConfiguration()
        {
            // Get the configuration name
            string configurationName = GetConfigFile();

            // If the file doesn't exist. Write the default (null if object) to it.
            if (!File.Exists(configurationName))
            {
                WriteConfig(default(T));
            }
        }

        /// <summary>
        /// Gets the configuration file full path depending on the configures type
        /// </summary>
        /// <returns>{ApplicationLocation}/Config/{TypeName}.json</returns>
        private static string GetConfigFile()
        {
            // Return the configuration file
            return $@"{AppDomain.CurrentDomain.BaseDirectory}/{configurationFolderName}/{typeof(T).Name}.json";
        }

        /// <summary>
        /// Read the created configuration file.
        /// </summary>
        /// <returns>The object of the configuration type</returns>
        public static T ReadConfig()
        {
            // Get the congfiguration File
            string configFile = GetConfigFile();

            // Deserialize Json to Object and Return It
            using (var fileStream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var textReader = new StreamReader(fileStream))
            {
                string content = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        /// <summary>
        /// Write to the configuration fil.
        /// </summary>
        /// <param name="configurationObject">The configuration object</param>
        public static void WriteConfig(T configurationObject)
        {
            // Grab the configuration file
            string configFile = GetConfigFile();

            // Serialize the object to JSON indented
            string fileContent = JsonConvert.SerializeObject(configurationObject, Formatting.Indented);

            // If it doesn't exist create the file and folder
            if (!Directory.Exists(Path.GetDirectoryName(configFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFile));
            }

            // Write the string into the config file
            using (StreamWriter writer = new StreamWriter(configFile, false, Encoding.Unicode))
            {
                writer.Write(fileContent);
            }
        }
    }
}
