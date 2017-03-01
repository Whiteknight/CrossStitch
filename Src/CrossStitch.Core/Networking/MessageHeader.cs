using System;

namespace CrossStitch.Core.Networking
{
    public class MessageHeader
    {
        public Guid MessageId { get; set; }
        public MessagePayloadType PayloadType { get; set; }
        public string EventName { get; set; }
        public TargetType FromType { get; set; }
        public Guid FromId { get; set; }
        public TargetType ToType { get; set; }
        public Guid ToId { get; set; }
        public string ZoneName { get; set; }
        public Guid? ProxyNodeId { get; set; }
    }
}
