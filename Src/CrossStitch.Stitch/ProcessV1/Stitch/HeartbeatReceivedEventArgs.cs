using System;

namespace CrossStitch.Stitch.ProcessV1.Stitch
{
    public class HeartbeatReceivedEventArgs : EventArgs
    {
        public long Id { get; set; }
    }
}
