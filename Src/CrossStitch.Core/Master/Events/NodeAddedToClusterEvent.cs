namespace CrossStitch.Core.Master.Events
{
    public class NodeAddedToClusterEvent
    {
        public const string EventName = "Added";
        public ClusterPeerNode Node { get; set; }
    }
}
