using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using System.Linq;
using System.Threading;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    // TODO: Some kind of request/response to get the current heartbeat ID, so we can calculate
    // if a stitch instance is running behind.
    public class StitchMonitorModule : IModule
    {
        private readonly NodeConfiguration _configuration;
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private DataHelperClient _data;
        private ModuleLog _log;
        private long _heartbeatId;

        public StitchMonitorModule(NodeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Name => ModuleNames.StitchMonitor;

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _subscriptions = new SubscriptionCollection(core.MessageBus);
            _data = new DataHelperClient(core.MessageBus);
            _log = new ModuleLog(_messageBus, Name);

            int heartbeatTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(heartbeatTickMultiple, b => b.Invoke(e => SendScheduledHeartbeat()));

            int monitorTickMultiple = (_configuration.StitchMonitorIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(monitorTickMultiple, b => b.Invoke(e => MonitorStitchStatus()));

            _subscriptions.Subscribe<StitchInstanceEvent>(b => b.WithChannelName(StitchInstanceEvent.ChannelSynced).Invoke(StitchSyncReceived));
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            Stop();
        }

        private void StitchSyncReceived(StitchInstanceEvent e)
        {
            long heartbeatId = e.DataId;
            _log.LogDebug("Stitch Id={0} Heartbeat sync received: {1}", e.InstanceId, heartbeatId);
            _data.Update<StitchInstance>(e.InstanceId, si =>
            {
                if (si.LastHeartbeatReceived < heartbeatId)
                    si.LastHeartbeatReceived = heartbeatId;
            });
        }

        private void SendScheduledHeartbeat()
        {
            long id = Interlocked.Increment(ref _heartbeatId);
            _log.LogDebug("Sending heartbeat {0}", id);
            var instances = _data.GetAllInstances()
                .Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started)
                .ToList(); ;

            foreach (var instance in instances)
            {
                var request = new InstanceRequest
                {
                    DataId = id,
                    Id = instance.Id,
                    Instance = instance
                };
                var result = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelSendHeartbeatVerified, request);
                if (!result.IsSuccess)
                    _data.Update<StitchInstance>(instance.Id, si => si.State = InstanceStateType.Missing);
            }
        }

        private void MonitorStitchStatus()
        {
            // TODO: Some kind of process to check the status of stitches, see how their LastHeartbeatId
            // compares to the current value, and send out alerts if things are looking unhealthy.
        }
    }
}
