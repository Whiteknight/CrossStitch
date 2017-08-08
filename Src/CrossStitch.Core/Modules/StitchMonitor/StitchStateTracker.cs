using System.Collections.Concurrent;
using CrossStitch.Core.Messages.StitchMonitor;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchStateTracker
    {
        private readonly IStitchHealthNotifier _notifier;
        private readonly StitchHealthCalculator _calculator;
        private readonly ConcurrentDictionary<string, long> _stitchLastSyncReceived;
        private readonly ConcurrentDictionary<string, bool> _stitchHealth;

        public StitchStateTracker(IStitchHealthNotifier notifier, StitchHealthCalculator calculator)
        {
            _notifier = notifier;
            _calculator = calculator;
            _stitchLastSyncReceived = new ConcurrentDictionary<string, long>();
            _stitchHealth = new ConcurrentDictionary<string, bool>();
        }

        public void ReceiveSync(long heartbeatId, string instanceId)
        {
            _stitchLastSyncReceived.AddOrUpdate(instanceId, x => heartbeatId, (x, l) => heartbeatId);
            bool returnedToHealth = _stitchHealth.TryUpdate(instanceId, true, false);
            if (returnedToHealth)
                _notifier.NotifyReturnToHealth(instanceId);
        }

        public void DetectUnhealthyStitches(long currentHeartbeatId)
        {
            foreach (var kvp in _stitchLastSyncReceived.ToArray())
            {
                var instanceId = kvp.Key;
                var lastSyncReceived = kvp.Value;
                bool isHealthy = _stitchHealth.GetOrAdd(instanceId, true);

                // It's already marked unhealthy. We shouldn't send duplicate notifications
                if (!isHealthy)
                    continue;

                // Determine if we're at the threshold for marking it unhealthy
                var report = _calculator.CalculateHealth(currentHeartbeatId, lastSyncReceived);
                if (report.HealthType != StitchHealthType.Red)
                    continue;

                if (_stitchHealth.TryUpdate(instanceId, false, true))
                    _notifier.NotifyUnhealthy(instanceId);
            }
        }

        public long GetLastHeartbeatSync(string instanceId)
        {
            bool ok = _stitchLastSyncReceived.TryGetValue(instanceId, out long lastSyncReceived);
            if (!ok || lastSyncReceived < 0)
                return -1;
            return lastSyncReceived;
        }

        public void StitchStarted(string id, long currentHeartbeatId)
        {
            _stitchLastSyncReceived.GetOrAdd(id, x => currentHeartbeatId);
            _stitchHealth.GetOrAdd(id, x => true);
        }

        public void StitchStopped(string id)
        {
            _stitchLastSyncReceived.TryRemove(id, out long value);
            _stitchHealth.TryRemove(id, out bool healthy);
        }
    }
}