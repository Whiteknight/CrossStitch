using CrossStitch.Stitch;
using CrossStitch.Stitch.BuiltInClassV1;

namespace Monitoring.Server
{
    public class MonitoringBuiltInStitch : IHandlesStart, IHandlesHeartbeat
    {
        private CoreStitchContext _context;

        public bool ReceiveHeartbeat(long id)
        {
            if (id < 5)
                return false;
            return true;
        }

        public bool Start(CoreStitchContext context)
        {
            _context = context;
            return true;
        }
    }
}
