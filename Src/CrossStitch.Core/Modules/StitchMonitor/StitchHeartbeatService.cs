using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchHeartbeatService
    {
        private readonly IModuleLog _log;
        private readonly IHeartbeatSender _sender;
        private readonly StitchStateTracker _tracker;
        private readonly HeartbeatSequence _sequence;
        private readonly StitchHealthCalculator _calculator;

        public StitchHeartbeatService(IModuleLog log, IHeartbeatSender sender, IStitchHealthNotifier notifier, StitchHealthCalculator calculator)
        {
            _log = log;
            _sender = sender;
            _sequence = new HeartbeatSequence();
            _calculator = calculator;
            _tracker = new StitchStateTracker(notifier, _calculator);
        }

        public long GetCurrentHeartbeatId()
        {
            return _sequence.GetCurrentHeartbeatId();
        }

        public void StitchSyncReceived(StitchInstanceEvent e)
        {
            long heartbeatId = e.DataId;

            _tracker.ReceiveSync(heartbeatId, e.InstanceId);

            _log.LogDebug("Stitch Id={0} Heartbeat sync received: {1}", e.InstanceId, heartbeatId);
        }

        public void SendScheduledHeartbeat()
        {
            var id = _sequence.MoveToNextHeartbeatId();
            _log.LogDebug("Sending heartbeat {0}", id);

            _sender.SendHeartbeat(id);

            _tracker.DetectUnhealthyStitches(id);
        }

        public StitchHealthResponse GetStitchHealthReport(StitchHealthRequest arg)
        {
            var heartbeatId = _sequence.GetCurrentHeartbeatId();
            var lastSyncReceived = _tracker.GetLastHeartbeatSync(arg.StitchId);
            var report = _calculator.CalculateHealth(heartbeatId, lastSyncReceived);
            return StitchHealthResponse.Create(arg, report.LastHeartbeatSync, heartbeatId, report.HealthType);
        }

        public void StitchStarted(string id)
        {
            _tracker.StitchStarted(id, GetCurrentHeartbeatId());
        }

        public void StitchStopped(string id)
        {
            _tracker.StitchStopped(id);
        }
    }
}
