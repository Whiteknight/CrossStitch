using Acquaintance;
using Microsoft.Extensions.Logging;
using CrossStitch.Core.Messages.Logging;

namespace CrossStitch.Core.Modules.Logging
{
    // This module serves as an adaptor between the ILog and IMessageBus
    public class LoggingModule : IModule
    {
        private readonly ILogger _log;
        private readonly SubscriptionCollection _subscriptions;

        private int _threadId;

        public LoggingModule(CrossStitchCore core, ILogger log)
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
                .Invoke(LogDebug)
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelInformation)
                .Invoke(LogInformation)
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelWarning)
                .Invoke(LogWarning)
                .OnThread(_threadId));

            _subscriptions.Subscribe<LogEvent>(b => b
                .WithTopic(LogEvent.LevelError)
                .Invoke(LogError)
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

        private void LogDebug(LogEvent l)
        {
            _log.LogDebug(l.Message);
        }

        private void LogInformation(LogEvent l)
        {
            _log.LogInformation(l.Message);
        }

        private void LogWarning(LogEvent l)
        {
            _log.LogWarning(l.Message);
        }

        private void LogError(LogEvent l)
        {
            if (l.Exception != null)
                _log.LogError(l.Exception, l.Message);
            else
                _log.LogError(l.Message);
        }
    }
}
