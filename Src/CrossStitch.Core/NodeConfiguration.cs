using CrossStitch.Core.Configuration;

namespace CrossStitch.Core
{
    public class NodeConfiguration : IModuleConfiguration
    {
        public static NodeConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<NodeConfiguration>("node.json");
        }

        public void ValidateAndSetDefaults()
        {
            if (HeartbeatIntervalMinutes <= 0)
                HeartbeatIntervalMinutes = 1;
            if (StitchMonitorIntervalMinutes <= 0)
                StitchMonitorIntervalMinutes = 5;
        }

        public int HeartbeatIntervalMinutes { get; set; }
        public int StitchMonitorIntervalMinutes { get; set; }
    }
}