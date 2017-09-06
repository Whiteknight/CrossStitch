using CrossStitch.Stitch.Utility;
using System.Configuration;
using System.IO;

namespace CrossStitch.Core.Configuration
{
    public static class ConfigurationLoader
    {
        private static readonly string[] DefaultSearchPaths = {
            @"Configs",
            @"Configuration"
        };

        private static string FindConfigFile(string fileName)
        {
            var herePath = Path.Combine(".", fileName);
            System.Console.WriteLine($"Searching for {herePath} (CWD={System.IO.Directory.GetCurrentDirectory()})");
            if (File.Exists(herePath))
                return herePath;
            foreach (var searchPath in DefaultSearchPaths)
            {
                string fullPath = Path.Combine(".", searchPath, fileName);
                System.Console.WriteLine($"Searching for {fullPath} (CWD={System.IO.Directory.GetCurrentDirectory()})");
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
