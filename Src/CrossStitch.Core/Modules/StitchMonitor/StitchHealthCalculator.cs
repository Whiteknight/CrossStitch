using CrossStitch.Core.Messages.StitchMonitor;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchHealthCalculator
    {
        private readonly int _missedHeartbeatsThreshold;

        public StitchHealthCalculator(int missedHeartbeatsThreshold)
        {
            _missedHeartbeatsThreshold = missedHeartbeatsThreshold;
        }

        public StitchHealthReport CalculateHealth(long currentHeartbeatId, long lastHeartbeatId)
        {
            if (lastHeartbeatId < 0)
                return new StitchHealthReport(0, 0, StitchHealthType.Missing);
            var missedHeartbeats = currentHeartbeatId - lastHeartbeatId;
            var type = CalculateHealthType(missedHeartbeats);
            return new StitchHealthReport(lastHeartbeatId, missedHeartbeats, type);
        }

        public StitchHealthType CalculateHealthType(long missedHeartbeats)
        {
            if (missedHeartbeats <= 1)
                return StitchHealthType.Green;
            if (missedHeartbeats <= _missedHeartbeatsThreshold)
                return StitchHealthType.Yellow;
            return StitchHealthType.Red;
        }
    }
}