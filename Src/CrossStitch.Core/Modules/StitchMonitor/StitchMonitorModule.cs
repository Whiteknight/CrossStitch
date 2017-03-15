using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;
using System.Linq;
using System.Threading;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchMonitorModule : IModule
    {
        private readonly NodeConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly DataHelperClient _data;
        private readonly ModuleLog _log;

        private SubscriptionCollection _subscriptions;
        private long _heartbeatId;

        public StitchMonitorModule(CrossStitchCore core, NodeConfiguration configuration)
        {
            _messageBus = core.MessageBus;
            _configuration = configuration;
            _data = new DataHelperClient(core.MessageBus);
            _log = new ModuleLog(_messageBus, Name);
        }

        public string Name => ModuleNames.StitchMonitor;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            int heartbeatTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(heartbeatTickMultiple, b => b.Invoke(e => SendScheduledHeartbeat()));

            int monitorTickMultiple = (_configuration.StitchMonitorIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(monitorTickMultiple, b => b.Invoke(e => MonitorStitchStatus()));

            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelSynced)
                .Invoke(StitchSyncReceived));

            _subscriptions.Listen<StitchHealthRequest, StitchHealthResponse>(l => l
                .OnDefaultChannel()
                .Invoke(GetStitchHealthReport));
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { "CurrentHeartbeatId", _heartbeatId.ToString() }
            };
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
            var instances = _data.GetAll<StitchInstance>()
                .Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started)
                .ToList();

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

        private StitchHealthResponse GetStitchHealthReport(StitchHealthRequest arg)
        {
            var stitch = _data.Get<StitchInstance>(arg.StitchId);
            if (stitch == null)
                return StitchHealthResponse.Create(arg, StitchHealthType.Missing);

            var health = StitchHealthResponse.CalculateHealth(_heartbeatId, stitch.LastHeartbeatReceived);
            return StitchHealthResponse.Create(arg, health);
        }
    }
}
