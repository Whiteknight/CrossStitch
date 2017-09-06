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
        private readonly StitchHeartbeatService _heartbeatService;
        private readonly SubscriptionCollection _subscriptions;

        public StitchMonitorModule(CrossStitchCore core, NodeConfiguration configuration)
        {
            _configuration = configuration;
            var log = new ModuleLog(core.MessageBus, Name);
            var calculator = new StitchHealthCalculator(configuration.MissedHeartbeatsThreshold);
            var heartbeatSender = new HeartbeatSender(core.MessageBus);
            var healthNotifier = new StitchHealthNotifier(core.MessageBus);
            _heartbeatService = new StitchHeartbeatService(log, heartbeatSender, healthNotifier, calculator);
            _subscriptions = new SubscriptionCollection(core.MessageBus);
        }

        public string Name => ModuleNames.StitchMonitor;

        public void Start()
        {
            int heartbeatTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe("tick", heartbeatTickMultiple, b => b
                .Invoke(e => _heartbeatService.SendScheduledHeartbeat()));

            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithTopic(StitchInstanceEvent.ChannelSynced)
                .Invoke(_heartbeatService.StitchSyncReceived));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithTopic(StitchInstanceEvent.ChannelStarted)
                .Invoke(m => _heartbeatService.StitchStarted(m.InstanceId)));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithTopic(StitchInstanceEvent.ChannelStopped)
                .Invoke(m => _heartbeatService.StitchStopped(m.InstanceId)));

            _subscriptions.Listen<StitchHealthRequest, StitchHealthResponse>(l => l
                .WithDefaultTopic()
                .Invoke(_heartbeatService.GetStitchHealthReport));
        }

        public void Stop()
        {
            _subscriptions.Dispose();
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
