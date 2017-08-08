using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Data.Folders
{
    public class Configuration : IModuleConfiguration
    {
        public static Configuration GetDefault()
        {
            var config = ConfigurationLoader.TryGetConfiguration<Configuration>("data.json");
            if (config == null)
            {
                config = new Configuration();
                config.ValidateAndSetDefaults();
            }
            return config;
        }

        public string DataPath { get; set; }

        public void ValidateAndSetDefaults()
        {

        }
    }
}
