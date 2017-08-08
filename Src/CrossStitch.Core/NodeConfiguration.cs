using CrossStitch.Core.Configuration;

namespace CrossStitch.Core
{
    public class NodeConfiguration : IModuleConfiguration
    {
        public static NodeConfiguration GetDefault()
        {
            var config = ConfigurationLoader.TryGetConfiguration<NodeConfiguration>("node.json");
            if (config != null)
                return config;
            config = new NodeConfiguration();
            config.ValidateAndSetDefaults();
            return config;
        }

        public string NodeId { get; set; }
        public string NodeName { get; set; }

        public int MissedHeartbeatsThreshold { get; set; }
        public int HeartbeatIntervalMinutes { get; set; }
        public int StatusBroadcastIntervalMinutes { get; set; }
        public string StateFileFolder { get; set; }

        public void ValidateAndSetDefaults()
        {
            if (HeartbeatIntervalMinutes <= 0)
                HeartbeatIntervalMinutes = 1;
            if (MissedHeartbeatsThreshold <= 0)
                MissedHeartbeatsThreshold = 5;
            if (StatusBroadcastIntervalMinutes <= 0)
                StatusBroadcastIntervalMinutes = 5;
            if (string.IsNullOrEmpty(StateFileFolder))
                StateFileFolder = ".";
        }
    }
}