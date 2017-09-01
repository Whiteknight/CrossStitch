using CrossStitch.Stitch;
using CrossStitch.Stitch.BuiltInClassV1;

namespace Monitoring.Server
{
    public class MonitoringBuiltInStitch : IHandlesStart, IHandlesHeartbeat
    {
        private IStitchEventObserver _observer;

        public bool ReceiveHeartbeat(long id)
        {
            if (id < 5)
                return false;
            return true;
        }

        public bool Start(IStitchEventObserver observer)
        {
            _observer = observer;
            return true;
        }
    }
}
