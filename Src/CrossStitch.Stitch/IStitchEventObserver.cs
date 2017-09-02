namespace CrossStitch.Stitch
{
    public interface IStitchEventObserver
    {
        void StitchStateChanged(string instanceId, bool isRunning, bool wasRequested);
        void MessageResponseReceived(string instanceId, long messageId, bool success);
        void LogsReceived(string instanceId, string[] logs);
        void HeartbeatSyncReceived(string instanceId, long heartbeatId);
        void DataMessageReceived(string instanceId, long messageId, string toGroup, string toInstanceId, string channelName, string data);
    }
}
