namespace CrossStitch.Stitch
{
    public interface IStitchEventObserver
    {
        void StitchInstancesOnStitchStateChanged(string instanceId, bool isRunning, bool wasRequested);
        void StitchInstanceManagerOnRequestResponseReceived(string instanceId, long messageId, bool success);
        void StitchInstanceManagerOnLogsReceived(string instanceId, string[] logs);
        void StitchInstanceManagerOnHeartbeatReceived(string instanceId, long heartbeatId);
        void StitchInstanceManagerOnDataMessageReceived(string instanceId, long messageId, string toGroup, string toInstanceId, string channelName, string data);
    }
}
