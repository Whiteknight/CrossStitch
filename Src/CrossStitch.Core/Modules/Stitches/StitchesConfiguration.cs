using CrossStitch.Core.Configuration;
using System.Collections.Generic;

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
        public Dictionary<string, string> ExtensionRunners { get; set; }

        public void ValidateAndSetDefaults()
        {
            if (string.IsNullOrEmpty(DataBasePath))
                DataBasePath = ".\\StitchData";
            if (string.IsNullOrEmpty(AppLibraryBasePath))
                AppLibraryBasePath = ".\\StitchLibrary";
            if (string.IsNullOrEmpty(RunningAppBasePath))
                RunningAppBasePath = ".\\RunningStitches";

            if (ExtensionRunners == null)
                ExtensionRunners = new Dictionary<string, string>();
        }
    }
}