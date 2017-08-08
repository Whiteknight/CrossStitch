using CrossStitch.Core.Messages.StitchMonitor;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchHealthReport
    {
        public StitchHealthReport(long lastHeartbeatSync, long missedHeartbeats, StitchHealthType healthType)
        {
            LastHeartbeatSync = lastHeartbeatSync;
            MissedHeartbeats = missedHeartbeats;
            HealthType = healthType;
        }

        public long LastHeartbeatSync { get; }
        public long MissedHeartbeats { get;  }
        public StitchHealthType HealthType { get;  }
    }
}