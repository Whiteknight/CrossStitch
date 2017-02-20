using System.Collections.Generic;
using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Modules.Backplane
{
    public class BackplaneConfiguration
    {
        public static BackplaneConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<BackplaneConfiguration>("backplane.json");
        }

        public int ListenPort { get; set; }
        public double SendTimeoutMs { get; set; }
        public List<string> Zones { get; set; }
    }
}