using CrossStitch.Core.Messages.Backplane;
using System;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public class MessageHeader
    {
        public Guid MessageId { get; set; }
        public MessagePayloadType PayloadType { get; set; }
        public string EventName { get; set; }

        public TargetType FromType { get; set; }
        public string FromEntityId { get; set; }
        public string FromNodeId { get; set; }
        public string FromNetworkId { get; set; }

        public TargetType ToType { get; set; }
        public string ToEntityId { get; set; }
        public string ToNetworkId { get; set; }
        public string ToNodeId { get; set; }

        public string ZoneName { get; set; }
        public string ProxyNodeNetworkId { get; set; }

        public void PopulateReceivedEvent(ReceivedEvent receivedEvent)
        {
            if (receivedEvent == null)
                return;
            receivedEvent.FromNetworkId = FromNetworkId;
            receivedEvent.FromNodeId = FromNodeId;
            receivedEvent.MessageId = MessageId;
            receivedEvent.ToNetworkId = ToNetworkId;
            receivedEvent.ToNodeId = ToNodeId;
        }
    }
}
