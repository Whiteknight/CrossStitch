using CrossStitch.Stitch.BuiltInClassV1;
using CrossStitch.Stitch.ProcessV1.Core;

namespace StitchStart.Server
{

    public class StitchStartBuiltInStitch : IHandlesStart, IHandlesHeartbeat
    {
        private CoreStitchContext _context;

        public bool ReceiveHeartbeat(long id)
        {
            return true;
        }

        public bool Start(CoreStitchContext context)
        {
            _context = context;
            return true;
        }
    }
}
