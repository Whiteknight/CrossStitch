namespace CrossStitch.Core.Modules.StitchMonitor
{
    public interface IHeartbeatSender
    {
        void SendHeartbeat(long heartbeatId);
    }
}