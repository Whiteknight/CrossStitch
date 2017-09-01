using CrossStitch.Stitch;
using CrossStitch.Stitch.BuiltInClassV1;

namespace StitchStart.Server
{

    public class StitchStartBuiltInStitch : IHandlesStart, IHandlesHeartbeat
    {
        private IStitchEventObserver _observer;

        public bool ReceiveHeartbeat(long id)
        {
            return true;
        }

        public bool Start(IStitchEventObserver observer)
        {
            _observer = observer;
            return true;
        }
    }
}
