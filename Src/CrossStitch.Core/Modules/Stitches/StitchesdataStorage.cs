using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesDataStorage : DataHelperClient
    {
        public StitchesDataStorage(IMessageBus messageBus)
            : base(messageBus)
        {
        }

        public StitchInstance GetInstance(string id)
        {
            return Get<StitchInstance>(id);
        }



        public Application GetApplication(string id)
        {
            return Get<Application>(id);
        }

        public void MarkHeartbeatSync(string id)
        {
            Update<StitchInstance>(id, si => si.MissedHeartbeats = 0);
        }

        public void MarkHeartbeatMissed(string id)
        {
            // TODO: Should we threshold here and set the status, or do that calculation later?
            Update<StitchInstance>(id, si => si.MissedHeartbeats++);
        }
    }
}
