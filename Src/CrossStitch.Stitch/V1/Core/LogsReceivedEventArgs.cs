using System;

namespace CrossStitch.Stitch.V1.Core
{
    public class LogsReceivedEventArgs : EventArgs
    {
        public string StitchInstanceId { get; set; }
        public string[] Logs { get; set; }

        public LogsReceivedEventArgs(string stitchInstanceId, string[] logs)
        {
            StitchInstanceId = stitchInstanceId;
            Logs = logs;
        }
    }
}