namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceRequest
    {
        public const string ChannelStart = "Start";
        public const string ChannelStop = "Stop";
        public const string ChannelClone = "Clone";
        public const string ChannelDelete = "Delete";
        public const string ChannelCreate = "Create";
        public const string ChannelSendHeartbeat = "SendHeartbeat";

        public string Id { get; set; }
        public long DataId { get; set; }
    }

    public class SendHeartbeatEvent
    {
        public SendHeartbeatEvent(long heartbeatId)
        {
            HeartbeatId = heartbeatId;
        }

        public long HeartbeatId { get; }
    }
}