using System;

namespace CrossStitch.Core.Modules.Backplane.Events
{
    public class ClusterMemberEvent
    {
        public const string ExitingEvent = "Exiting";
        public const string EnteringEvent = "Entering";
        public Guid NodeUuid { get; set; }
        public string NodeName { get; set; }
    }
}