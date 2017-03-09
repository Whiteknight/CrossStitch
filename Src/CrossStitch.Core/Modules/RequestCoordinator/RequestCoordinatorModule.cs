using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using System.Linq;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public class RequestCoordinatorModule : IModule
    {
        private SubscriptionCollection _subscriptions;
        private CrossStitchCore _node;
        private IMessageBus _messageBus;
        private DataHelperClient _data;
        private ModuleLog _log;

        public string Name => ModuleNames.RequestCoordinator;

        public void Start(CrossStitchCore core)
        {
            _node = core;
            _subscriptions = new SubscriptionCollection(core.MessageBus);
            _messageBus = core.MessageBus;
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

            // CRUD requests for Components, which are all updates on Application records
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Insert).Invoke(InsertComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Update).Invoke(UpdateComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l.WithChannelName(ComponentChangeRequest.Delete).Invoke(DeleteComponent));

            // CRUD requests for Stitch Instances
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelCreate).Invoke(CreateNewInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelDelete).Invoke(DeleteInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelClone).Invoke(CloneInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelStart).Invoke(StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelStop).Invoke(StopInstance));

            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l.OnDefaultChannel().Invoke(UploadPackageFile));

            _log.LogDebug("Started");
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

        // On Core Initialization, get all stitch instances from the data store and start them.
        private void StartupStitches(CoreEvent obj)
        {
            _log.LogDebug("Starting startup stitches");
            var instances = _data.GetAllInstances();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started))
            {
                var result = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
                {
                    Id = instance.Id
                });
                // TODO: Do something with the result? The updated instance is already saved by the
                // Stitches module at this point
            }
            _log.LogDebug("Startup stitches started");
        }

        private GenericResponse DeleteComponent(ComponentChangeRequest arg)
        {
            // TODO: Should we delete all stitches of this component? If so, we need to get a list
            // of all stitches in this component, stop and delete each, and then update our record
            // here.
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                a.RemoveComponent(arg.Name);
            });
            return new GenericResponse(application != null);
        }

        // TODO: This method doesn't make sense. We wouldn't change the name of a Component.
        // Remove this if there is no other use-case.
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
                updated = a.AddComponent(arg.Name);
            });
            return new GenericResponse(application != null && updated);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            // TODO: Should we delete stitches from this application? If so, we'll need to get a 
            // list of all stitches from the Data module and stop all of them, delete them,
            // and then delete the application.
            bool ok = _data.Delete<Application>(arg.Id);
            return new GenericResponse(ok);
        }

        private Application UpdateApplication(ApplicationChangeRequest arg)
        {
            return _data.Update<Application>(arg.Id, a => a.Name = arg.Name);
        }

        // TODO: We also need to broadcast these messages out over the backplane so other nodes keep
        // track of applications. Actually, this might be included with node status broadcasts.
        // Investigate further.

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
            // Tell the Stitches module to stop the Stitch
            var stopResponse = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStop, request);
            if (!stopResponse.IsSuccess)
                _log.LogError("Instance {0} could not be stopped", request.Id);

            // Delete the record from the Data module
            bool deleted = _data.Delete<StitchInstance>(request.Id);
            if (!deleted)
                _log.LogError("Instance {0} could not be deleted", request.Id);

            // TODO: Update the Core status object to remove this from the list of running stitches.

            if (stopResponse.IsSuccess && deleted)
                _log.LogInformation("Instance {0} stopped and deleted successfully", request.Id);
            return new InstanceResponse
            {
                IsSuccess = stopResponse.IsSuccess && deleted
            };
        }

        private InstanceResponse CloneInstance(InstanceRequest request)
        {
            // Get a copy of the instance data from the Data module
            var instance = _data.Get<StitchInstance>(request.Id);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, instance does not exist.", request.Id);
                return InstanceResponse.Failure(request);
            }

            // Update the model to be fresh
            instance.Id = null;
            instance.StoreVersion = 0;

            // Insert the fresh version to the Data module
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, data could not be saved.", request.Id);
                return InstanceResponse.Failure(request);
            }

            // Report success
            _log.LogInformation("Instance {0} cloned to {1}", request.Id, instance.Id);
            return InstanceResponse.Success(request);
        }

        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            // Get the application and make sure we have a Component record
            var application = _data.Get<Application>(request.ApplicationId);
            if (application == null)
                return new PackageFileUploadResponse(false, null);
            if (application.Components.All(c => c.Name != request.Component))
                return new PackageFileUploadResponse(false, null);
            request.Application = application;

            // Save the file and generate a unique Version name
            var response = _messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(PackageFileUploadRequest.ChannelUpload, request);
            if (!response.Success)
            {
                _log.LogDebug("Package file upload  {0}:{1} failed", request.ApplicationId, request.Component);
                return new PackageFileUploadResponse(false, null);
            }

            // Update the Application record with the new Version
            _data.Update<Application>(request.ApplicationId, a => a.AddVersion(request.Component, response.Version));
            _log.LogDebug("Uploaded package file {0}:{1}:{2}", request.ApplicationId, request.Component, response.Version);
            return new PackageFileUploadResponse(true, response.Version);
        }

        private InstanceResponse CreateNewInstance(InstanceRequest request)
        {
            if (request == null || request.Instance == null || string.IsNullOrEmpty(request.Instance.Application))
                return InstanceResponse.Failure(request);
            // Check to make sure we have a record for this version.
            Application application = _data.Get<Application>(request.Instance.Application);
            if (application == null)
                return InstanceResponse.Failure(request);
            if (!application.HasVersion(request.Instance.Component, request.Instance.Version))
                return InstanceResponse.Failure(request);

            // Insert the new instance to the data module
            request.Instance.Id = null;
            request.Instance.StoreVersion = 0;
            request.Instance = _data.Insert(request.Instance);

            // Perform the actual create logic in the Stitches module
            var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelCreateVerified, request);
            if (!response.IsSuccess)
            {
                _log.LogDebug("Stitch instance {0}:{1} could ot be created", request.Instance.Application, request.Instance.Component);
                _data.Delete<StitchInstance>(request.Instance.Id);
                return InstanceResponse.Failure(request);
            }

            // Update the stitch record in the Data module, log, and return
            var directoryPath = response.Data;
            var instance = _data.Update<StitchInstance>(request.Instance.Id, i => i.DirectoryPath = directoryPath);
            _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} created", request.Instance.Application, request.Instance.Component, request.Instance.Version, request.Instance.Id);
            return InstanceResponse.Success(request);
        }

        private InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            if (instance == null)
                return InstanceResponse.Failure(request);
            request.Instance = instance;

            var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStartVerified, request);
            if (response.IsSuccess)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelStarted, new StitchInstanceEvent
                {
                    InstanceId = request.Id
                });
                _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} started", instance.Application, instance.Component, instance.Version, instance.Id);
            }
            else
            {
                _log.LogError(response.Exception, "Stitch instance {0}:{1}:{2} Id={3} failed to start", instance.Application, instance.Component, instance.Version, instance.Id);
            }

            // The Stitches module will update status in all cases, so always save
            _data.Save<StitchInstance>(instance);
            return response;
        }

        private InstanceResponse StopInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            if (instance == null)
                return InstanceResponse.Failure(request);
            request.Instance = instance;

            var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStopVerified, request);
            if (!response.IsSuccess)
            {
                _log.LogError("Stitch instance {0}:{1}:{2} Id={3} could not be stopped", instance.Application, instance.Component, instance.Version, instance.Id);
                return response;
            }

            _messageBus.Publish(StitchInstanceEvent.ChannelStopped, new StitchInstanceEvent
            {
                InstanceId = request.Id
            });
            var updateResult = _data.Update<StitchInstance>(request.Id, i => i.State = InstanceStateType.Stopped);
            _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} stopped", instance.Application, instance.Component, instance.Version, instance.Id);
            return InstanceResponse.Create(request, updateResult != null);
        }
    }
}
