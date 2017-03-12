using System.Collections.Generic;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public class MessageEnvelope
    {
        public const string SendEventName = "Send";
        public const string ReceiveEventName = "Received";

        public MessageEnvelope()
        {
            Header = new MessageHeader
            {
                PayloadType = MessagePayloadType.None
            };
        }

        public MessageHeader Header { get; set; }
        public List<string> CommandStrings { get; set; }
        public object PayloadObject { get; set; }
        public List<byte[]> RawFrames { get; set; }

        public string GetReceiveEventName()
        {
            if (Header == null || string.IsNullOrEmpty(Header.EventName))
                return ReceiveEventName;
            return $"{ReceiveEventName}:{Header.EventName}";
        }
    }
}