using System.Configuration;
using System.IO;
using CrossStitch.Core.Communications;
using CrossStitch.Core.Master;

namespace CrossStitch.Core.Configuration
{
    public static class ConfigurationLoader
    {
        private static string FindConfigFile(string fileName)
        {
            string basePath = ConfigurationManager.AppSettings["Configuration:BasePath"];
            if (!string.IsNullOrEmpty(basePath))
            {
                string path = Path.Combine(basePath, fileName);
                if (File.Exists(path))
                    return path;
                return null;
            }

            string fullPath = Path.Combine(".\\Configs\\", fileName);
            if (File.Exists(fullPath))
                return fullPath;

            return null;
        }

        public static TConfig GetConfiguration<TConfig>(string fileName)
        {
            string fullPath = FindConfigFile(fileName);
            if (string.IsNullOrEmpty(fullPath))
                throw new ConfigurationException("Could not find configuration file " + fileName);

            string json = File.ReadAllText(fullPath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TConfig>(json);
        }
    }
}
