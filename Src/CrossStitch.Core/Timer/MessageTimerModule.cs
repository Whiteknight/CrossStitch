using Acquaintance;
using CrossStitch.Core.Node;
using System;

namespace CrossStitch.Core.Timer
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

        public MessageTimerModule(IMessageBus messageBus, int delaySeconds)
        {
            if (delaySeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(delaySeconds), "delaySeconds must be 1 or higher");
            _timer = new Acquaintance.Timers.MessageTimer(5000, delaySeconds * 1000);
            _messageBus = messageBus;
        }

        public string Name => "Timer";

        public void Start(RunningNode context)
        {
            if (_token != null)
                return;
            _token = _messageBus.AddModule(_timer);
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
