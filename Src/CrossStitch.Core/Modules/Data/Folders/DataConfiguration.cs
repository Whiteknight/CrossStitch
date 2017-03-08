using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Data
{
    public class Configuration
    {
        public static Configuration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<Configuration>("data.json");
        }

        public string DataPath { get; set; }
    }
}
