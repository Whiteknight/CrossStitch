using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesConfiguration : IModuleConfiguration
    {
        public static StitchesConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<StitchesConfiguration>("stitches.json");
        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }

        public void ValidateAndSetDefaults()
        {
            if (string.IsNullOrEmpty(DataBasePath))
                DataBasePath = ".\\StitchData";
            if (string.IsNullOrEmpty(AppLibraryBasePath))
                AppLibraryBasePath = ".\\StitchLibrary";
            if (string.IsNullOrEmpty(RunningAppBasePath))
                RunningAppBasePath = ".\\RunningStitches";
        }
    }
}