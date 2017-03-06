using System;

namespace CrossStitch.Stitch.V1.Core
{
    public class RequestResponseReceivedEventArgs : EventArgs
    {
        private readonly string _stitchInstanceId;
        public long MessageId { get; set; }
        public bool WasSuccess { get; set; }

        public RequestResponseReceivedEventArgs(string stitchInstanceId, long messageId, bool wasSuccess)
        {
            _stitchInstanceId = stitchInstanceId;
            MessageId = messageId;
            WasSuccess = wasSuccess;
        }
    }
}