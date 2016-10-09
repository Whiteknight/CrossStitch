using System.Collections.Generic;
using CrossStitch.Core.Communications;
using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Master
{
    public enum NodeDetectionType
    {
        StaticList,
        DynamicBroadcast
    }

    public class MasterConfiguration
    {
        public static MasterConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<MasterConfiguration>("master.json");
        }

        public int ListenPort { get; set; }
        public NodeDetectionType NodeDetection { get; set; }
        public double PingTimeoutMs { get; set; }
        public List<NodeCommunicationInformation> NodeList { get; set; }
    }
}
