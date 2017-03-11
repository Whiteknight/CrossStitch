using CrossStitch.Stitch.Utility;
using System.Configuration;
using System.IO;

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
                return File.Exists(path) ? path : null;
            }

            string fullPath = Path.Combine(".\\Configs\\", fileName);
            if (File.Exists(fullPath))
                return fullPath;

            return null;
        }

        public static TConfig GetConfiguration<TConfig>(string fileName)
            where TConfig : IModuleConfiguration
        {
            string fullPath = FindConfigFile(fileName);
            // TODO: If the file can't be found, we should show some kind of warning but
            // provide a default
            if (string.IsNullOrEmpty(fullPath))
                throw new ConfigurationException("Could not find configuration file " + fileName);

            string json = File.ReadAllText(fullPath);
            var config = JsonUtility.Deserialize<TConfig>(json);
            config.ValidateAndSetDefaults();
            return config;
        }
    }
}
