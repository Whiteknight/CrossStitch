using System;
using CrossStitch.Stitch.Events;

namespace CrossStitch.Stitch.ProcessV1.Core
{
    public class CoreStitchContext
    {
        public CoreStitchContext(string stitchId)
        {
            StitchInstanceId = stitchId;
        }

        public event EventHandler<StitchProcessEventArgs> StitchStateChange;
        public event EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public event EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public event EventHandler<LogsReceivedEventArgs> LogsReceived;
        public event EventHandler<DataMessageReceivedEventArgs> DataMessageReceived;

        public long LastHeartbeatReceived { get; private set; }

        public string StitchInstanceId { get; }
        public string DataDirectory { get; set; }

        public void RaiseProcessEvent(bool isRunning, bool wasRequested)
        {
            StitchStateChange.Raise(this, new StitchProcessEventArgs(StitchInstanceId, isRunning, wasRequested));
        }

        public void ReceiveHeartbeat(long id)
        {
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

        public void ReceiveData(long messageId, string toGroupName, string toStitchInstanceId, string channelName, string data)
        {
            DataMessageReceived.Raise(this, new DataMessageReceivedEventArgs
            {
                MessageId = messageId,
                ToGroupName = toGroupName,
                ToStitchInstanceId = toStitchInstanceId,
                FromStitchInstanceId = StitchInstanceId,
                ChannelName = channelName,
                Data = data
            });
        }
    }
}
