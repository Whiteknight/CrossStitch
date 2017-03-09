using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceRequest
    {
        public const string ChannelStart = "Start";
        public const string ChannelStartVerified = "StartVerified";
        public const string ChannelStop = "Stop";
        public const string ChannelStopVerified = "StopVerified";
        public const string ChannelClone = "Clone";
        public const string ChannelDelete = "Delete";
        public const string ChannelCreate = "Create";
        public const string ChannelCreateVerified = "CreateVerified";
        public const string ChannelSendHeartbeat = "SendHeartbeat";
        public const string ChannelSendHeartbeatVerified = "SendHeartbeatVerified";

        public string Id { get; set; }
        public StitchInstance Instance { get; set; }
        public long DataId { get; set; }
    }
}