using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public class RequestCoordinatorModule : IModule
    {
        private readonly CrossStitchCore _core;
        private readonly IMessageBus _messageBus;
        private readonly CoordinatorService _service;

        private SubscriptionCollection _subscriptions;

        public RequestCoordinatorModule(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _core = core;
            var log = new ModuleLog(_messageBus, Name);
            var data = new DataHelperClient(_messageBus);
            _service = new CoordinatorService(_messageBus, data, log);
        }

        public string Name => ModuleNames.RequestCoordinator;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            // On Core initialization, startup all necessary Stitches
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(m => _service.StartRunningStitchesOnStartup())
                .OnWorkerThread());

            // CRUD requests for Application records
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Insert)
                .Invoke(arg => _service.CreateApplication(arg.Name)));
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Update)
                .Invoke(m => _service.UpdateApplication(m.Id, m.Name)));
            _subscriptions.Listen<ApplicationChangeRequest, GenericResponse>(l => l
                .WithChannelName(ApplicationChangeRequest.Delete)
                .Invoke(DeleteApplication));

            // CRUD requests for Components, which are all updates on Application records
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Insert)
                .Invoke(InsertComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Update)
                .Invoke(UpdateComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Delete)
                .Invoke(DeleteComponent));

            // CRUD requests for Stitch Instances
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelCreate)
                .Invoke(_service.CreateNewInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelDelete)
                .Invoke(DeleteInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelClone)
                .Invoke(CloneInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStart)
                .Invoke(_service.StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStop)
                .Invoke(_service.StopInstance));

            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l
                .OnDefaultChannel()
                .Invoke(_service.UploadStitchPackageFile));
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>();
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

        private GenericResponse DeleteComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.DeleteComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        // TODO: This method doesn't make sense. We wouldn't change the name of a Component.
        // Remove this if there is no other use-case.
        private GenericResponse UpdateComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.UpdateComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        private GenericResponse InsertComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.InsertComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            bool ok = _service.DeleteApplication(arg.Id);
            return new GenericResponse(ok);
        }

        private InstanceResponse DeleteInstance(InstanceRequest request)
        {
            bool ok = _service.DeleteStitchInstance(request.Id);
            return InstanceResponse.Create(request, ok);
        }

        private InstanceResponse CloneInstance(InstanceRequest request)
        {
            var instance = _service.CloneStitchInstance(request.Id);
            if (instance == null)
                return InstanceResponse.Failure(request);
            return InstanceResponse.Success(request);
        }
    }
}
