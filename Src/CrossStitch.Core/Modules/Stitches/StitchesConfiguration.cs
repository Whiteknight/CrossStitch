using CrossStitch.Core.Configuration;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesConfiguration : IModuleConfiguration
    {
        public static StitchesConfiguration GetDefault()
        {
            var config = ConfigurationLoader.TryGetConfiguration<StitchesConfiguration>("stitches.json");
            if (config == null)
            {
                System.Console.WriteLine("COULD NOT FIND STITCHES CONFIG");
                config = new StitchesConfiguration();
                config.ValidateAndSetDefaults();
            }
            return config;
        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }
        public Dictionary<string, StitchesExtensionConfiguration> Extensions { get; set; }

        public void ValidateAndSetDefaults()
        {
            if (string.IsNullOrEmpty(DataBasePath))
                DataBasePath = ".\\StitchData";
            if (string.IsNullOrEmpty(AppLibraryBasePath))
                AppLibraryBasePath = ".\\StitchLibrary";
            if (string.IsNullOrEmpty(RunningAppBasePath))
                RunningAppBasePath = ".\\RunningStitches";

            if (Extensions == null)
                Extensions = new Dictionary<string, StitchesExtensionConfiguration>();
        }
    }

    public class StitchesExtensionConfiguration
    {
        public string ExecutableFormat { get; set; }
        public string ArgumentsFormat { get; set; }
    }
}