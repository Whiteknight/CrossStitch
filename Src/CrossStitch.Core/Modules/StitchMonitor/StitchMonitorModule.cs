using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class StitchMonitorModule : IModule
    {
        private readonly NodeConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly StitchHeartbeatService _heartbeatService;

        private SubscriptionCollection _subscriptions;

        public StitchMonitorModule(CrossStitchCore core, NodeConfiguration configuration)
        {
            _messageBus = core.MessageBus;
            _configuration = configuration;
            var data = new DataHelperClient(core.MessageBus);
            var log = new ModuleLog(_messageBus, Name);
            _heartbeatService = new StitchHeartbeatService(data, log, new HeartbeatSender(_messageBus));
        }

        public string Name => ModuleNames.StitchMonitor;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            int heartbeatTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(heartbeatTickMultiple, b => b
                .Invoke(e => _heartbeatService.SendScheduledHeartbeat()));

            int monitorTickMultiple = (_configuration.StitchMonitorIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(monitorTickMultiple, b => b.Invoke(e => MonitorStitchStatus()));

            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelSynced)
                .Invoke(_heartbeatService.StitchSyncReceived));

            _subscriptions.Listen<StitchHealthRequest, StitchHealthResponse>(l => l
                .OnDefaultChannel()
                .Invoke(_heartbeatService.GetStitchHealthReport));
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
                { "CurrentHeartbeatId", _heartbeatService.GetCurrentHeartbeatId().ToString() }
            };
        }

        public void Dispose()
        {
            Stop();
        }

        private void MonitorStitchStatus()
        {
            // TODO: Some kind of process to check the status of stitches, see how their LastHeartbeatId
            // compares to the current value, and send out alerts if things are looking unhealthy.
        }

        private class HeartbeatSender : IHeartbeatSender
        {
            private readonly IMessageBus _messageBus;

            public HeartbeatSender(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public bool SendHeartbeat(StitchInstance instance, long heartbeatId)
            {
                var request = new EnrichedInstanceRequest(instance.Id, instance)
                {
                    DataId = heartbeatId
                };
                var result = _messageBus.Request<EnrichedInstanceRequest, InstanceResponse>(InstanceRequest.ChannelSendHeartbeat, request);
                return result.IsSuccess;
            }
        }
    }
}
