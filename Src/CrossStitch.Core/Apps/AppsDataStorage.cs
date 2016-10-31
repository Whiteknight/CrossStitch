using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Apps
{
    public class AppsDataStorage
    {
        private readonly IMessageBus _messageBus;

        public AppsDataStorage(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public IEnumerable<Instance> GetAllInstances()
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.GetAll())
                .Responses
                .FirstOrDefault(r => r.Type == DataResponseType.Success);
            if (response == null)
                return Enumerable.Empty<Instance>();

            return response.Entities;
        }

        public Instance GetInstance(string id)
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.Get(id))
                .Responses
                .FirstOrDefault(r => r.Type == DataResponseType.Success);
            if (response == null)
                return null;
            return response.Entity;
        }

        public bool Save(Instance instance)
        {
            var response = _messageBus
                .Request<DataRequest<Instance>, DataResponse<Instance>>(DataRequest<Instance>.Save(instance))
                .Responses
                .FirstOrDefault(r => r.Type == DataResponseType.Success);
            if (response == null)
                return false;
            return true;
        }

        public Application GetApplication(string id)
        {
            var response = _messageBus
                .Request<DataRequest<Application>, DataResponse<Application>>(DataRequest<Application>.Get(id))
                .Responses
                .FirstOrDefault(r => r.Type == DataResponseType.Success);
            if (response == null)
                return null;
            return response.Entity;
        }
    }
}
