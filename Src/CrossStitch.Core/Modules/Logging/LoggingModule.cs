using Acquaintance;
using Common.Logging;
using CrossStitch.Core.Messages.Logging;

namespace CrossStitch.Core.Modules.Logging
{
    public class LoggingModule : IModule
    {
        private SubscriptionCollection _subscriptions;
        private readonly ILog _log;
        private int _threadId;
        private IMessageBus _messageBus;

        public LoggingModule(ILog log)
        {
            _log = log;
        }

        public string Name => ModuleNames.Log;

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _subscriptions = new SubscriptionCollection(core.MessageBus);

            _threadId = core.MessageBus.ThreadPool.StartDedicatedWorker();

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithChannelName(LogEvent.LevelDebug)
                .Invoke(l => _log.Debug(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithChannelName(LogEvent.LevelInformation)
                .Invoke(l => _log.Info(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithChannelName(LogEvent.LevelWarning)
                .Invoke(l => _log.Warn(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithChannelName(LogEvent.LevelError)
                .Invoke(l =>
                {
                    if (l.Exception != null)
                        _log.Error(l.Message, l.Exception);
                    else
                        _log.Error(l.Message);
                })
                .OnThread(_threadId));
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;

            _messageBus?.ThreadPool?.StopDedicatedWorker(_threadId);
            _threadId = 0;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
            };
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
