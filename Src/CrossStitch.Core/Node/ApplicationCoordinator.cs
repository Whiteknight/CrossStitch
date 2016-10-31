using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Node.Messages;
using System.Linq;

namespace CrossStitch.Core.Node
{
    public class ApplicationCoordinator : IModule
    {
        private SubscriptionCollection _subscriptions;
        private RunningNode _node;
        private IMessageBus _messageBus;

        public string Name => "ApplicationCoordinator";

        public void Start(RunningNode context)
        {
            _node = context;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _messageBus = context.MessageBus;

            _subscriptions.Listen<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, CreateApplication);
            _subscriptions.Listen<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Update, UpdateApplication);
            _subscriptions.Listen<ApplicationChangeRequest, GenericResponse>(ApplicationChangeRequest.Delete, DeleteApplication);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            var request = DataRequest<Application>.Delete(arg.Id);
            var response = _messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
            return new GenericResponse(response.Responses.Any(dr => dr.Type == DataResponseType.Success));
        }

        private Application UpdateApplication(ApplicationChangeRequest arg)
        {
            var getResponse = _messageBus.Request<DataRequest<Application>, DataResponse<Application>>(DataRequest<Application>.Get(arg.Id));
            var applicationEntity = getResponse.Responses.Select(dr => dr.Entity).FirstOrDefault();
            if (applicationEntity == null)
                return null;

            applicationEntity.Name = arg.Name;
            var response = _messageBus.Request<DataRequest<Application>, DataResponse<Application>>(DataRequest<Application>.Save(applicationEntity));
            return response.Responses.Select(r => r.Entity).FirstOrDefault();
        }

        // TODO: We also need to broadcast these messages out over the backplane so other nodes keep
        // track of applications.

        private Application CreateApplication(ApplicationChangeRequest arg)
        {
            // TODO: Check that an application with the same name doesn't already exist
            Application application = new Application
            {
                Name = arg.Name,
                StoreVersion = 0,
                NodeId = _node.NodeId
            };
            var request = DataRequest<Application>.Save(application);
            var response = _messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
            return response.Responses.Select(dr => dr.Entity).FirstOrDefault();
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
