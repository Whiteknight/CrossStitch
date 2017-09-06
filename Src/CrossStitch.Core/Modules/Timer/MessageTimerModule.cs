using Acquaintance;
using CrossStitch.Core.MessageBus;
using System;
using System.Threading;

namespace CrossStitch.Core.Modules.Timer
{
    public class MessageTimerModule : IModule
    {
        public const int TimerIntervalSeconds = 10;

        private readonly IMessageBus _messageBus;
        private readonly int _intervalSeconds;
        private readonly ModuleLog _log;

        private RegisteredWaitHandle _waitHandle;
        private ManualResetEvent _stop;
        private long _sequence;

        public MessageTimerModule(IMessageBus messageBus)
            : this(messageBus, TimerIntervalSeconds)
        {
        }

        public MessageTimerModule(IMessageBus messageBus, int intervalSeconds)
        {
            if (intervalSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "intervalSeconds must be 1 or higher");

            _intervalSeconds = intervalSeconds;
            //_timer = new Acquaintance.Timers.MessageTimer(delayMs: 5000, intervalMs: intervalSeconds * 1000);
            _messageBus = messageBus;
            _sequence = 0;
            _log = new ModuleLog(messageBus, Name);
        }

        public string Name => ModuleNames.Timer;

        public void Start()
        {
            _stop = new ManualResetEvent(false);
            _waitHandle = ThreadPool.RegisterWaitForSingleObject(_stop, TimerTick, null, 1000 * _intervalSeconds, false);
        }

        public void Stop()
        {
            _stop.Set();
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            long sequenceId = Interlocked.Read(ref _sequence);
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { "SequenceNumber", sequenceId.ToString() }
            };
        }

        public void Dispose()
        {
            Stop();
            _waitHandle = null;
        }

        private void TimerTick(object state, bool timedOut)
        {
            if (!timedOut)
                return;

            try
            {
                long sequence = Interlocked.Increment(ref _sequence);
                _messageBus.Publish("tick", new Acquaintance.Timers.MessageTimerEvent(null, sequence));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error publishing tick");
            }
        }
    }
}
