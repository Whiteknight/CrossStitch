using System;

namespace CrossStitch.Stitch.ProcessV1.Core
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

    public class DataMessageReceivedEventArgs : EventArgs
    {
        public long MessageId { get; set; }
        public string ToGroupName { get; set; }
        public string ToStitchInstanceId { get; set; }
        public string FromStitchInstanceId { get; set; }
        public string ChannelName { get; set; }
        public string Data { get; set; }
    }
}