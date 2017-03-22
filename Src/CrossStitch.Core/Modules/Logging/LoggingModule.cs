using Acquaintance;
using Common.Logging;
using CrossStitch.Core.Messages.Logging;

namespace CrossStitch.Core.Modules.Logging
{
    // This module serves as an adaptor between the ILog and IMessageBus
    public class LoggingModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly ILog _log;

        private SubscriptionCollection _subscriptions;
        private int _threadId;

        public LoggingModule(CrossStitchCore core, ILog log)
        {
            _log = log;
            _messageBus = core.MessageBus;
        }

        public string Name => ModuleNames.Log;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            _threadId = _messageBus.ThreadPool.StartDedicatedWorker();

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
            return new System.Collections.Generic.Dictionary<string, string>();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
