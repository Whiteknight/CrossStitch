using System;
using System.Threading;
using CrossStitch.Core.Messaging;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Timer
{
    public class MessageTimerModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly int _delaySeconds;
        private System.Threading.Timer _timer;
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
            _delaySeconds = delaySeconds;
            _messageId = 0;
        }

        public string Name { get { return "Timer";  } }

        public void Start(RunningNode context)
        {
            _timer = new System.Threading.Timer(TimerTick, null, 5000, _delaySeconds * 1000);
        }

        public void Stop()
        {
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }

        private void TimerTick(object state)
        {
            var id = Interlocked.Increment(ref _messageId);
            _messageBus.Publish(MessageTimerEvent.EventName, new MessageTimerEvent(id));
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
