using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Data
{
    public class DataConfiguration
    {
        public static DataConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<DataConfiguration>("data.json");
        }

        public string DataPath { get; set; }
    }
}
