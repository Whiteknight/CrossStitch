using System;

namespace CrossStitch.Core.Modules.Stitches.Messages
{
    public class AppInstanceEvent
    {
        public const string AddedEventName = "Added";
        public const string StartedEventName = "Started";
        public const string StoppedEventName = "Stopped";
        public const string RemovedEventName = "Removed";
        
        public Guid NodeId { get; set; }
        public string InstanceId { get; set; }
    }
}
