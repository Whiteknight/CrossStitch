using System;

namespace CrossStitch.Core.Messages
{
    public class StitchDataMessage
    {
        public const string ChannelSendLocal = "SendLocal";
        public long Id { get; set; }

        public string DataChannelName { get; set; }
        public string Data { get; set; }

        public string ToStitchInstanceId { get; set; }
        public string ToStitchGroup { get; set; }
        public string ToNetworkId { get; set; }
        public string ToNodeId { get; set; }

        public string FromStitchInstanceId { get; set; }
        public string FromNetworkId { get; set; }
        public string FromNodeId { get; set; }
    }
}
