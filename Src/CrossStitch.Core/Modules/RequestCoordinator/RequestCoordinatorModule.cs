using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using System;

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
            if (core == null)
                throw new ArgumentNullException(nameof(core));

            _messageBus = core.MessageBus;
            _core = core;

            var log = new ModuleLog(_messageBus, Name);
            var data = new DataHelperClient(_messageBus);
            _service = new CoordinatorService(data, log, new StitchRequestHandler(_messageBus, log), new StitchEventNotifier(_messageBus));
        }

        public RequestCoordinatorModule(CrossStitchCore core, CoordinatorService service)
        {
            if (core == null)
                throw new ArgumentNullException(nameof(core));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _messageBus = core.MessageBus;
            _core = core;
            _service = service;
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
            _subscriptions.Listen<CreateInstanceRequest, InstanceResponse>(l => l
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
            return InstanceResponse.Create(request, instance != null);
        }

        private class StitchRequestHandler : IStitchRequestHandler
        {
            private readonly IMessageBus _messageBus;
            private readonly IModuleLog _log;

            public StitchRequestHandler(IMessageBus messageBus, IModuleLog log)
            {
                _messageBus = messageBus;
                _log = log;
            }

            public StitchInstance StartInstance(StitchInstance instance)
            {
                var request = new EnrichedInstanceRequest(instance);
                var response = _messageBus.Request<EnrichedInstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, request);
                if (!response.IsSuccess)
                    _log.LogError(response.Exception, "Stitch instance {0} Id={1} failed to start", instance.GroupName, instance.Id);
                return response.Instance;
            }

            public StitchInstance StopInstance(StitchInstance instance)
            {
                var request = new EnrichedInstanceRequest(instance);
                var response = _messageBus.Request<EnrichedInstanceRequest, InstanceResponse>(InstanceRequest.ChannelStop, request);
                if (!response.IsSuccess)
                    _log.LogError(response.Exception, "Stitch instance {0} Id={1} failed to stop", instance.GroupName, instance.Id);
                return response.Instance;
            }

            public StitchInstance CreateInstance(StitchInstance instance)
            {
                var instanceRequest = new EnrichedInstanceRequest(instance);
                var response = _messageBus.Request<EnrichedInstanceRequest, InstanceResponse>(InstanceRequest.ChannelCreate, instanceRequest);
                if (response.IsSuccess == false)
                    return null;
                instance.Adaptor = response.Instance.Adaptor;
                return instance;
            }

            public PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request)
            {
                return _messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(PackageFileUploadRequest.ChannelUpload, request);
            }
        }

        private class StitchEventNotifier : IStitchEventNotifier
        {
            private readonly IMessageBus _messageBus;

            public StitchEventNotifier(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void StitchStarted(StitchInstance instance)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelStarted, new StitchInstanceEvent
                {
                    InstanceId = instance.Id
                });
            }

            public void StitchStopped(StitchInstance instance)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelStopped, new StitchInstanceEvent
                {
                    InstanceId = instance.Id
                });
            }
        }
    }
}
