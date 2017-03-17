using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using System.Linq;
using System.Threading;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchHeartbeatService
    {
        private readonly IDataRepository _data;
        private readonly IModuleLog _log;
        private readonly IHeartbeatSender _sender;

        private long _heartbeatId;

        public StitchHeartbeatService(IDataRepository data, IModuleLog log, IHeartbeatSender sender)
        {
            _data = data;
            _log = log;
            _sender = sender;
            _heartbeatId = 0;
        }

        public long GetCurrentHeartbeatId()
        {
            long id = Interlocked.Read(ref _heartbeatId);
            return id;
        }

        public void StitchSyncReceived(StitchInstanceEvent e)
        {
            long heartbeatId = e.DataId;
            _log.LogDebug("Stitch Id={0} Heartbeat sync received: {1}", e.InstanceId, heartbeatId);
            _data.Update<StitchInstance>(e.InstanceId, si =>
            {
                if (si.LastHeartbeatReceived < heartbeatId)
                    si.LastHeartbeatReceived = heartbeatId;
            });
        }

        public void SendScheduledHeartbeat()
        {
            long id = Interlocked.Increment(ref _heartbeatId);
            _log.LogDebug("Sending heartbeat {0}", id);
            var instances = _data.GetAll<StitchInstance>()
                .Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started)
                .ToList();

            foreach (var instance in instances)
            {
                var ok = _sender.SendHeartbeat(instance, id);
                if (!ok)
                    _data.Update<StitchInstance>(instance.Id, si => si.State = InstanceStateType.Missing);
            }
        }

        public StitchHealthResponse GetStitchHealthReport(StitchHealthRequest arg)
        {
            var heartbeatId = GetCurrentHeartbeatId();
            var stitch = _data.Get<StitchInstance>(arg.StitchId);
            if (stitch == null)
                return StitchHealthResponse.Create(arg, StitchHealthType.Missing);

            var health = StitchHealthResponse.CalculateHealth(heartbeatId, stitch.LastHeartbeatReceived);
            return StitchHealthResponse.Create(arg, health);
        }
    }
}
