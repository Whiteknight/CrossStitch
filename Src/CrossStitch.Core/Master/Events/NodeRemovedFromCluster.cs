namespace CrossStitch.Core.Master.Events
{
    public class NodeRemovedFromClusterEvent
    {
        public const string EventName = "Removed";
        public ClusterPeerNode Node { get; set; }
    }
}