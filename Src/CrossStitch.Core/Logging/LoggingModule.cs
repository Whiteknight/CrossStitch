using Acquaintance;
using Common.Logging;
using CrossStitch.Core.Logging.Events;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Logging
{
    public class LoggingModule : IModule
    {
        private SubscriptionCollection _subscriptions;
        private readonly ILog _log;
        private int _threadId;
        private IMessageBus _messageBus;

        public LoggingModule(ILog log = null)
        {
            _log = log ?? LogManager.GetLogger("CrossStitch");
        }

        public string Name => "Logging";

        public void Start(CrossStitchCore context)
        {
            _messageBus = context.MessageBus;
            _subscriptions = new SubscriptionCollection(context.MessageBus);

            _threadId = context.MessageBus.ThreadPool.StartDedicatedWorker();

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

        public void Dispose()
        {
            Stop();
        }
    }
}
