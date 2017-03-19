using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Messages.Backplane
{
    public class ClusterMessage
    {
        public const string SendEventName = "Send";
        public const string ReceiveEventName = "Received";

        public ClusterMessage()
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
            if (string.IsNullOrEmpty(Header?.EventName))
                return ReceiveEventName;
            return $"{ReceiveEventName}:{Header.EventName}";
        }

        public bool IsSendable()
        {
            if (Header == null)
                return false;
            if (Header.PayloadType == MessagePayloadType.Object && PayloadObject == null)
                return false;
            if (Header.PayloadType == MessagePayloadType.CommandString && (CommandStrings == null || !CommandStrings.Any() || CommandStrings.All(string.IsNullOrWhiteSpace)))
                return false;
            if (Header.PayloadType == MessagePayloadType.Raw && (RawFrames == null || !RawFrames.Any() || RawFrames.All(rf => rf.Length == 0)))
                return false;
            return Header.IsSendable();
        }
    }
}