using CrossStitch.Core.Configuration;

namespace CrossStitch.Core
{
    public class NodeConfiguration : IModuleConfiguration
    {
        public static NodeConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<NodeConfiguration>("node.json");
        }

        public string NodeId { get; set; }
        public string NodeName { get; set; }
        public int HeartbeatIntervalMinutes { get; set; }
        public int StitchMonitorIntervalMinutes { get; set; }
        public int StatusBroadcastIntervalMinutes { get; set; }
        public string StateFileFolder { get; set; }

        public void ValidateAndSetDefaults()
        {
            if (HeartbeatIntervalMinutes <= 0)
                HeartbeatIntervalMinutes = 1;
            if (StitchMonitorIntervalMinutes <= 0)
                StitchMonitorIntervalMinutes = 5;
            if (StatusBroadcastIntervalMinutes <= 0)
                StatusBroadcastIntervalMinutes = 5;
            if (string.IsNullOrEmpty(StateFileFolder))
                StateFileFolder = ".";
        }
    }
}