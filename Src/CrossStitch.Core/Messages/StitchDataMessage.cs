using System;

namespace CrossStitch.Core.Messages
{
    public class StitchDataMessage
    {
        public const string ChannelSend = "Send";
        public const string ChannelSendEnriched = "SendEnriched";
        public const string ChannelSendLocal = "SendLocal";

        public string ChannelName { get; set; }

        public string StitchInstanceId { get; set; }
        public string StitchGroup { get; set; }
        public string Data { get; set; }

        public string FromStitchId { get; set; }
        public string FromNetworkId { get; set; }
        public Guid FromNodeId { get; set; }

        public string ToNetworkId { get; set; }
        public Guid ToNodeId { get; set; }
    }
}
