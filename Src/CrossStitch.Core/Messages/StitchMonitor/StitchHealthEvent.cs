namespace CrossStitch.Core.Messages.StitchMonitor
{
    public class StitchHealthEvent
    {
        public const string TopicUnhealthy = "unhealthy";
        public const string TopicReturnToHealth = "healthy";

        public string InstanceId { get; set; }
        
        // TODO: Information about number of missed heartbeats or other stats?
    }
}