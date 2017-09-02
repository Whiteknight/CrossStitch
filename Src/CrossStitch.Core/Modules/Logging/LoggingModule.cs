using Acquaintance;
using Common.Logging;
using CrossStitch.Core.Messages.Logging;

namespace CrossStitch.Core.Modules.Logging
{
    // This module serves as an adaptor between the ILog and IMessageBus
    public class LoggingModule : IModule
    {
        private readonly ILog _log;
        private readonly SubscriptionCollection _subscriptions;

        private int _threadId;

        public LoggingModule(CrossStitchCore core, ILog log)
        {
            _log = log;
            _subscriptions = new SubscriptionCollection(core.MessageBus);
        }

        public string Name => ModuleNames.Log;

        public void Start()
        {
            _subscriptions.Clear();
            _threadId = _subscriptions.WorkerPool.StartDedicatedWorker().ThreadId;
            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelDebug)
                .Invoke(l => _log.Debug(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelInformation)
                .Invoke(l => _log.Info(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelWarning)
                .Invoke(l => _log.Warn(l.Message))
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelError)
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
            _subscriptions.Dispose();
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
