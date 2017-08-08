using CrossStitch.Stitch.Utility;
using System.Configuration;
using System.IO;

namespace CrossStitch.Core.Configuration
{
    public static class ConfigurationLoader
    {
        private static readonly string[] DefaultSearchPaths = {
            @".\Configs\",
            @".\Configuration\",
            @".\"
        };

        private static string FindConfigFile(string fileName)
        {
            string basePath = ConfigurationManager.AppSettings["Configuration:BasePath"];
            if (!string.IsNullOrEmpty(basePath))
            {
                string path = Path.Combine(basePath, fileName);
                return File.Exists(path) ? path : null;
            }

            foreach (var searchPath in DefaultSearchPaths)
            {
                string fullPath = Path.Combine(searchPath, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static TConfig GetConfiguration<TConfig>(string fileName)
            where TConfig : IModuleConfiguration
        {
            var config = TryGetConfiguration<TConfig>(fileName);
            if (config == null)
                throw new ConfigurationException("Could not find configuration file " + fileName);
            return config;
        }

        public static TConfig TryGetConfiguration<TConfig>(string fileName)
            where TConfig : IModuleConfiguration
        {
            string fullPath = FindConfigFile(fileName);
            if (string.IsNullOrEmpty(fullPath))
                return default(TConfig);

            string json = File.ReadAllText(fullPath);
            var config = JsonUtility.Deserialize<TConfig>(json);
            config.ValidateAndSetDefaults();
            return config;
        }
    }
}
