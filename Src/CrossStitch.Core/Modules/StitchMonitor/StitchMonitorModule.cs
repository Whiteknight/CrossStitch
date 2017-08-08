using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;

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
            var log = new ModuleLog(_messageBus, Name);
            var calculator = new StitchHealthCalculator(configuration.MissedHeartbeatsThreshold);
            var heartbeatSender = new HeartbeatSender(_messageBus);
            var healthNotifier = new StitchHealthNotifier(_messageBus);
            _heartbeatService = new StitchHeartbeatService(log, heartbeatSender, healthNotifier, calculator);
        }

        public string Name => ModuleNames.StitchMonitor;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            int heartbeatTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(heartbeatTickMultiple, b => b
                .Invoke(e => _heartbeatService.SendScheduledHeartbeat()));

            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelSynced)
                .Invoke(_heartbeatService.StitchSyncReceived));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelStarted)
                .Invoke(m => _heartbeatService.StitchStarted(m.InstanceId)));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelStopped)
                .Invoke(m => _heartbeatService.StitchStopped(m.InstanceId)));

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

        private class HeartbeatSender : IHeartbeatSender
        {
            private readonly IMessageBus _messageBus;

            public HeartbeatSender(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void SendHeartbeat(long heartbeatId)
            {
                _messageBus.Publish(new SendHeartbeatEvent(heartbeatId));
            }
        }

        private class StitchHealthNotifier : IStitchHealthNotifier
        {
            private readonly IMessageBus _messageBus;

            public StitchHealthNotifier(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void NotifyUnhealthy(string instanceId)
            {
                _messageBus.Publish(StitchHealthEvent.TopicUnhealthy, new StitchHealthEvent
                {
                    InstanceId = instanceId
                });
            }

            public void NotifyReturnToHealth(string instanceId)
            {
                _messageBus.Publish(StitchHealthEvent.TopicReturnToHealth, new StitchHealthEvent
                {
                    InstanceId = instanceId
                });
            }
        }
    }
}
