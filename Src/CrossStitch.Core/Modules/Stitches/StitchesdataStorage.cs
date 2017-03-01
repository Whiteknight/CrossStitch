using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using CrossStitch.Core.Node;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesDataStorage : DataHelperClient
    {
        public StitchesDataStorage(IMessageBus messageBus)
            : base(messageBus)
        {
        }

        public IEnumerable<StitchInstance> GetAllInstances()
        {
            var response = Bus
                .Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(DataRequest<StitchInstance>.GetAll());
            if (response == null)
                return Enumerable.Empty<StitchInstance>();

            return response.Entities;
        }

        public StitchInstance GetInstance(string id)
        {
            return Get<StitchInstance>(id);
        }

        public bool Save(StitchInstance stitchInstance)
        {

            var response = Bus
                .Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(DataRequest<StitchInstance>.Save(stitchInstance));
            return response != null;
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
