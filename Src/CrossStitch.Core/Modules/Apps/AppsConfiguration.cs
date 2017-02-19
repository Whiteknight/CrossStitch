using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Apps
{
    public class AppsConfiguration
    {
        public static AppsConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<AppsConfiguration>("apps.json");
        }

        public void SetDefaults()
        {
            
        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }
    }
}