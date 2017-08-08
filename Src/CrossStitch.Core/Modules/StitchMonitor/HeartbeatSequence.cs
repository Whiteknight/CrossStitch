using System.Threading;

namespace CrossStitch.Core.Modules.StitchMonitor
{
    public class HeartbeatSequence
    {
        private long _heartbeatId;

        public HeartbeatSequence()
        {
            _heartbeatId = 0;
        }

        public long GetCurrentHeartbeatId()
        {
            long id = Interlocked.Read(ref _heartbeatId);
            return id;
        }

        public long MoveToNextHeartbeatId()
        {
            return Interlocked.Increment(ref _heartbeatId);
        }
    }
}