using Acquaintance;
using CrossStitch.Core.Node;
using System;

namespace CrossStitch.Core.Timer
{
    public class MessageTimerModule : IModule
    {
        private readonly Acquaintance.Timers.MessageTimer _timer;

        public MessageTimerModule(IMessageBus messageBus)
            : this(messageBus, 10)
        {
        }

        public MessageTimerModule(IMessageBus messageBus, int delaySeconds)
        {
            if (delaySeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(delaySeconds), "delaySeconds must be 1 or higher");
            _timer = new Acquaintance.Timers.MessageTimer(messageBus, 5000, delaySeconds * 1000);
        }

        public string Name => "Timer";

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
