using System;

namespace CrossStitch.Core.Messages.Backplane
{
    public abstract class ReceivedEvent
    {
        public Guid MessageId { get; set; }
        public Guid FromNodeId { get; set; }
        public string FromNetworkId { get; set; }

        public string ToNetworkId { get; set; }
        public Guid ToNodeId { get; set; }

        public static string ReceivedEventName(string eventName)
        {
            return "Received:" + eventName;
        }
    }

    public class ObjectsReceivedEvent<TPayload> : ReceivedEvent
    {
        public TPayload Object { get; set; }
    }
}
