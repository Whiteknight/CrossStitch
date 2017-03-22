using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    // Event message for announcing events related to Stitch instances
    public class StitchInstanceEvent
    {
        public const string ChannelStarted = "Started";
        public const string ChannelStopped = "Stopped";
        public const string ChannelSynced = "SyncReceived";

        public string InstanceId { get; set; }
        public long DataId { get; set; }
        public StitchGroupName GroupName { get; set; }
    }
}
