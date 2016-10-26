using System;
using Acquaintance;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Timer
{
    public class MessageTimerModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly int _delaySeconds;
        private Acquaintance.Timers.MessageTimer _timer;
        private int _messageId;

        public MessageTimerModule(IMessageBus messageBus)
            : this(messageBus, 10)
        {
        }

        public MessageTimerModule(IMessageBus messageBus, int delaySeconds)
        {
            if (delaySeconds < 1)
                throw new ArgumentOutOfRangeException("delaySeconds", "delaySeconds must be 1 or higher");
            _messageBus = messageBus;
            _timer = new Acquaintance.Timers.MessageTimer(_messageBus, 5000, delaySeconds * 1000);
        }

        public string Name { get { return "Timer";  } }

        public void Start(RunningNode context)
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
