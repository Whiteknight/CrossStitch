using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Data.Folders
{
    public class Configuration : IModuleConfiguration
    {
        public static Configuration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<Configuration>("data.json");
        }

        public string DataPath { get; set; }
        public void ValidateAndSetDefaults()
        {
            throw new System.NotImplementedException();
        }
    }
}
