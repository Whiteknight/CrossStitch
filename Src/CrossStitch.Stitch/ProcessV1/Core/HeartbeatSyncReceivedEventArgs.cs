using System;

namespace CrossStitch.Stitch.ProcessV1.Core
{
    public class HeartbeatSyncReceivedEventArgs : EventArgs
    {
        public string StitchInstanceId { get; set; }
        public long Id { get; set; }

        public HeartbeatSyncReceivedEventArgs(string stitchInstanceId, long id)
        {
            StitchInstanceId = stitchInstanceId;
            Id = id;
        }
    }
}