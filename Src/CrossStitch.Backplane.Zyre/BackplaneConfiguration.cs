using CrossStitch.Core.Configuration;
using System.Collections.Generic;

namespace CrossStitch.Backplane.Zyre
{
    public class BackplaneConfiguration : IModuleConfiguration
    {
        public static BackplaneConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<BackplaneConfiguration>("backplane.json");
        }

        public int ListenPort { get; set; }
        public List<string> Zones { get; set; }

        public void ValidateAndSetDefaults()
        {
            // TODO: Default ListenPort
            if (Zones == null)
                Zones = new List<string>();
        }
    }
}