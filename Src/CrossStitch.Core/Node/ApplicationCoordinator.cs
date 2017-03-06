using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Core.Node.Messages;
using System.Linq;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules;

namespace CrossStitch.Core.Node
{
    public class ApplicationCoordinatorModule : IModule
    {
        // TODO: Move most of this mutable data into a state object.
        private SubscriptionCollection _subscriptions;
        private CrossStitchCore _node;
        private IMessageBus _messageBus;
        private DataHelperClient _data;
        private ModuleLog _log;

        public string Name => "ApplicationCoordinator";

        public void Start(CrossStitchCore context)
        {
            _node = context;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _messageBus = context.MessageBus;
            _log = new ModuleLog(_messageBus, Name);
            _data = new DataHelperClient(_messageBus);

            // On Core initialization, startup all necessary Stitches
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(StartupStitches)
                .OnWorkerThread());

            // CRUD requests for Application records
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Insert)
                .Invoke(CreateApplication));
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Update)
                .Invoke(UpdateApplication));
            _subscriptions.Listen<ApplicationChangeRequest, GenericResponse>(l => l
                .WithChannelName(ApplicationChangeRequest.Delete)
                .Invoke(DeleteApplication));

            // CRUD requests for Components
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Insert).Invoke(InsertComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Update).Invoke(UpdateComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Delete).Invoke(DeleteComponent));

            // CRUD requests for Stitch Instances
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Delete).Invoke(DeleteInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Clone).Invoke(CloneInstance));

            _log.LogDebug("Started");
        }

        // On Core Initialization, get all stitch instances from the data store and start them.
        private void StartupStitches(CoreEvent obj)
        {
            _log.LogDebug("Starting startup stitches");
            var instances = _data.GetAllInstances();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started))
            {
                var result = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Start, new InstanceRequest
                {
                    Id = instance.Id
                });
                _data.Save(instance);
            }
            _log.LogDebug("Startup stitches started");
        }

        private GenericResponse DeleteComponent(ComponentChangeRequest arg)
        {
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                a.Components = a.Components.Where(c => c.Name != arg.Name).ToList();
            });
            return new GenericResponse(application != null);
        }

        private GenericResponse UpdateComponent(ComponentChangeRequest arg)
        {
            bool updated = false;
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                updated = false;
                var component = a.Components.FirstOrDefault(c => c.Name == arg.Name);
                if (component != null)
                {
                    updated = true;
                    component.Name = arg.Name;
                }
            });
            return new GenericResponse(application != null && updated);
        }

        private GenericResponse InsertComponent(ComponentChangeRequest arg)
        {
            bool updated = false;
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                updated = false;
                var component = a.Components.FirstOrDefault(c => c.Name == arg.Name);
                if (component == null)
                {
                    a.Components.Add(new ApplicationComponent
                    {
                        Name = arg.Name
                    });
                }
            });
            return new GenericResponse(application != null && updated);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            bool ok = _data.Delete<Application>(arg.Id);
            return new GenericResponse(ok);
        }

        private Application UpdateApplication(ApplicationChangeRequest arg)
        {
            return _data.Update<Application>(arg.Id, a => a.Name = arg.Name);
        }

        // TODO: We also need to broadcast these messages out over the backplane so other nodes keep
        // track of applications.

        private Application CreateApplication(ApplicationChangeRequest arg)
        {
            // TODO: Check that an application with the same name doesn't already exist
            var application = _data.Insert(new Application
            {
                Name = arg.Name,
                NodeId = _node.NodeId
            });
            if (application != null)
                _log.LogInformation("Created application {0}:{1}", application.Id, application.Name);
            return application;
        }

        private InstanceResponse DeleteInstance(InstanceRequest request)
        {
            var stopResponse = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Stop, request);
            if (!stopResponse.Success)
                _log.LogError("Instance {0} could not be stopped", request.Id);
            bool deleted = _data.Delete<StitchInstance>(request.Id);
            if (!deleted)
                _log.LogError("Instance {0} could not be deleted", request.Id);
            if (stopResponse.Success && deleted)
                _log.LogInformation("Instance {0} stopped and deleted successfully", request.Id);
            return new InstanceResponse
            {
                Success = stopResponse.Success && deleted
            };
        }

        private InstanceResponse CloneInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            instance.Id = null;
            instance.StoreVersion = 0;
            instance = _data.Insert(instance);
            if (instance == null)
                _log.LogError("Could not clone instance {0}, data could not be saved.", request.Id);
            else
                _log.LogInformation("Instance {0} cloned to {1}", request.Id, instance.Id);

            return new InstanceResponse
            {
                Success = true,
                Id = instance.Id
            };
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
