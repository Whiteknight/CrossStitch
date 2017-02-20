using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesdataStorage
    {
        private readonly IMessageBus _messageBus;

        public StitchesdataStorage(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public IEnumerable<Instance> GetAllInstances()
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.GetAll());
            if (response == null)
                return Enumerable.Empty<Instance>();

            return response.Entities;
        }

        public Instance GetInstance(string id)
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.Get(id));
            return response?.Entity;
        }

        public bool Save(Instance instance)
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.Save(instance));
            return response != null;
        }

        public Application GetApplication(string id)
        {
            var response = _messageBus
                .Request<DataRequest<Application>, DataResponse<Application>>(DataRequest<Application>.Get(id));
            return response?.Entity;
        }
    }
}
