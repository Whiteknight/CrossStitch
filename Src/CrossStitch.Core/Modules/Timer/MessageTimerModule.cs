using System;
using Acquaintance;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Modules.Timer
{
    public class MessageTimerModule : IModule
    {
        private readonly Acquaintance.Timers.MessageTimer _timer;
        private IDisposable _token;
        private readonly IMessageBus _messageBus;

        public MessageTimerModule(IMessageBus messageBus)
            : this(messageBus, 10)
        {
        }

        public MessageTimerModule(IMessageBus messageBus, int intervalSeconds)
        {
            if (intervalSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "intervalSeconds must be 1 or higher");
            _timer = new Acquaintance.Timers.MessageTimer(delayMs: 5000, intervalMs: intervalSeconds * 1000);
            _messageBus = messageBus;
        }

        public string Name => "Timer";

        public void Start(RunningNode context)
        {
            if (_token != null)
                return;
            _token = _messageBus.Modules.Add(_timer);
        }

        public void Stop()
        {
            if (_token == null)
                return;
            _token.Dispose();
            _token = null;
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
        }
    }
}
