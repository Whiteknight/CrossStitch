using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public interface IHeartbeatSender
    {
        bool SendHeartbeat(StitchInstance instance, long heartbeatId);
    }
}