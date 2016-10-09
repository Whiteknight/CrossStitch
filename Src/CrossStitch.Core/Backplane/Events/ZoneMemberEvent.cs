using System;

namespace CrossStitch.Core.Backplane
{
    public class ZoneMemberEvent
    {
        public const string LeavingEvent = "Leaving";
        public const string JoiningEvent = "Joining";
        public Guid NodeUuid { get; set; }
        public string NodeName { get; set; }
        public string Zone { get; set; }
    }
}