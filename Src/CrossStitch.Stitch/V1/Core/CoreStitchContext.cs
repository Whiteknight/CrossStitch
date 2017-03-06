using CrossStitch.Stitch.Events;
using System;

namespace CrossStitch.Stitch.V1.Core
{
    public class CoreStitchContext
    {
        public event EventHandler<StitchProcessEventArgs> StitchStateChange;
        public event EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public event EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public event EventHandler<LogsReceivedEventArgs> LogsReceived;

        public long LastHeartbeatReceived { get; private set; }

        public string StitchInstanceId { get; }

        public CoreStitchContext(string stitchId)
        {
            StitchInstanceId = stitchId;
        }

        public void RaiseProcessEvent(bool isRunning, bool wasRequested)
        {
            StitchStateChange.Raise(this, new StitchProcessEventArgs(StitchInstanceId, isRunning, wasRequested));
        }

        public void ReceiveHeartbeat(long id)
        {
            // TODO: This seems like it might be un-thread-safe, but the consenquence of sending
            // heartbeats out of order are low, and the data-store thread is sync'd, so our persistance
            // operation won't have a problem.
            if (id <= LastHeartbeatReceived)
                return;
            LastHeartbeatReceived = id;
            HeartbeatReceived.Raise(this, new HeartbeatSyncReceivedEventArgs(StitchInstanceId, LastHeartbeatReceived));
        }

        public void ReceiveResponse(long messageId, bool success)
        {
            RequestResponseReceived.Raise(this, new RequestResponseReceivedEventArgs(StitchInstanceId, messageId, success));
        }

        public void ReceiveLogs(string[] logs)
        {
            if (logs == null || logs.Length == 0)
                return;
            LogsReceived.Raise(this, new LogsReceivedEventArgs(StitchInstanceId, logs));
        }

        public void ReceiveData()
        {
            // TODO: The Stitch should be able to send data (addressed to any other stitch in the same application)
            // or log messages (addressed to the core) without the Core sending a request first. This communication
            // should be fully bi-directional, not request/response.
        }
    }
}
