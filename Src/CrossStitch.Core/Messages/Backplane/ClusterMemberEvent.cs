using System;

namespace CrossStitch.Core.Messages.Backplane
{
    public class ClusterMemberEvent
    {
        public const string ExitingEvent = "Exiting";
        public const string EnteringEvent = "Entering";

        public string NetworkNodeId { get; set; }
        public string NodeId { get; set; }
    }
}