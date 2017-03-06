using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Models;

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

        public void MarkHeartbeatSync(string id, long heartbeatId)
        {
            Update<StitchInstance>(id, si =>
            {
                if (si.LastHeartbeatReceived < heartbeatId)
                    si.LastHeartbeatReceived = heartbeatId;
            });
        }
    }
}
