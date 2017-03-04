using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesConfiguration
    {
        public static StitchesConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<StitchesConfiguration>("stitches.json");
        }

        public void SetDefaults()
        {

        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }

        public int HeartbeatIntervalMinutes { get; set; }
    }
}