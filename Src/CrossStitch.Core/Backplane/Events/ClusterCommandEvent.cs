using System;

namespace CrossStitch.Core.Backplane
{
    public class ClusterCommandEvent
    {
        public const string EventName = "Received";
        public Guid NodeUuid { get; set; }
        public string NodeName { get; set; }
        public string Zone { get; set; }
        public string Command { get; set; }
        public object[] Frames { get; set; }
    }
}