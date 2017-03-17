using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using System.Linq;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public interface IStitchRequestHandler
    {
        StitchInstance StartInstance(StitchInstance instance);
        StitchInstance StopInstance(StitchInstance instance);
        StitchInstance CreateInstance(StitchInstance instance);
        PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request);
    }

    public interface IStitchEventNotifier
    {
        void StitchStarted(StitchInstance instance);
        void StitchStopped(StitchInstance instance);
    }
    
    public class CoordinatorService
    {
        private readonly IDataRepository _data;
        private readonly IModuleLog _log;
        private readonly IStitchRequestHandler _stitchRequests;
        private readonly IStitchEventNotifier _notifier;

        public CoordinatorService(IDataRepository data, IModuleLog log, IStitchRequestHandler stitchRequests, IStitchEventNotifier notifier)
        {
            _data = data;
            _log = log;
            _stitchRequests = stitchRequests;
            _notifier = notifier;
        }

        // On Core Initialization, get all stitch instances from the data store and start them.
        public void StartRunningStitchesOnStartup()
        {
            _log.LogDebug("Starting startup stitches");
            var instances = _data.GetAll<StitchInstance>();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started))
            {
                var startedInstance = _stitchRequests.StartInstance(instance);
                if (startedInstance.IsStartedOrRunning())
                    _notifier.StitchStarted(instance);
                else
                    _data.Save(startedInstance);
            }
            _log.LogDebug("Startup stitches started");
        }

        public Application CreateApplication(string name)
        {
            // TODO: Check that an application with the same name doesn't already exist
            var application = _data.Insert(new Application
            {
                Name = name
            });
            if (application != null)
                _log.LogInformation("Created application {0}:{1}", application.Id, application.Name);
            return application;
        }

        public Application UpdateApplication(string applicationId, string newName)
        {
            return _data.Update<Application>(applicationId, a => a.Name = newName);
        }

        public bool DeleteApplication(string applicationId)
        {
            // TODO: Should we delete stitches from this application? If so, we'll need to get a 
            // list of all stitches from the Data module and stop all of them, delete them,
            // and then delete the application.
            return _data.Delete<Application>(applicationId);
        }

        public bool DeleteComponent(string applicationId, string component)
        {
            // TODO: Should we delete all stitches of this component? If so, we need to get a list
            // of all stitches in this component, stop and delete each, and then update our record
            // here.
            Application application = _data.Update<Application>(applicationId, a =>
            {
                a.RemoveComponent(component);
            });
            return application != null;
        }

        public bool UpdateComponent(string applicationId, string component)
        {
            bool updated = false;
            Application application = _data.Update<Application>(applicationId, a =>
            {
                updated = false;
                var comp = a.Components.FirstOrDefault(c => c.Name == component);
                if (comp != null)
                {
                    updated = true;
                    comp.Name = component;
                }
            });
            return application != null && updated;
        }

        public bool InsertComponent(string applicationId, string component)
        {
            bool updated = false;
            Application application = _data.Update<Application>(applicationId, a =>
            {
                updated = a.AddComponent(component);
            });
            return application != null && updated;
        }

        public bool DeleteStitchInstance(string stitchInstanceId)
        {
            var instance = _data.Get<StitchInstance>(stitchInstanceId);
            if (instance == null)
                return false;

            // Tell the Stitches module to stop the Stitch
            var stoppedInstance = _stitchRequests.StopInstance(instance);
            if (stoppedInstance.State != InstanceStateType.Stopped)
            {
                _log.LogError("Instance {0} could not be stopped", stitchInstanceId);
                return false;
            }

            // Delete the record from the Data module
            bool deleted = _data.Delete<StitchInstance>(stitchInstanceId);
            if (!deleted)
            {
                _log.LogError("Instance {0} could not be deleted", stitchInstanceId);
                return false;
            }

            _log.LogInformation("Instance {0} stopped and deleted successfully", stitchInstanceId);

            return true;
        }

        public StitchInstance CloneStitchInstance(string stitchInstanceId)
        {
            // Get a copy of the instance data from the Data module
            var instance = _data.Get<StitchInstance>(stitchInstanceId);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, instance does not exist.", stitchInstanceId);
                return null;
            }

            // Update the model to be fresh
            instance.Id = null;
            instance.StoreVersion = 0;

            // Insert the fresh version to the Data module
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, data could not be saved.", stitchInstanceId);
                return null;
            }

            // Report success
            _log.LogInformation("Instance {0} cloned to {1}", stitchInstanceId, instance.Id);
            return instance;
        }

        public PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request)
        {
            // Get the application and make sure we have a Component record
            var application = _data.Get<Application>(request.ApplicationId);
            if (application == null)
                return new PackageFileUploadResponse(false, null);
            if (application.Components.All(c => c.Name != request.Component))
                return new PackageFileUploadResponse(false, null);
            request.Application = application;

            // Save the file and generate a unique Version name
            var response = _stitchRequests.UploadStitchPackageFile(request);
            if (!response.Success)
            {
                _log.LogDebug("Package file upload  {0}:{1} failed", request.ApplicationId, request.Component);
                return response;
            }

            // Update the Application record with the new Version
            _data.Update<Application>(request.ApplicationId, a => a.AddVersion(request.Component, response.Version));
            _log.LogDebug("Uploaded package file {0}:{1}:{2}", request.ApplicationId, request.Component, response.Version);
            return response;
        }

        // TODO: We also need to broadcast these messages out over the backplane so other nodes keep
        // track of applications. Actually, this might be included with node status broadcasts.
        // Investigate further.

        public InstanceResponse CreateNewInstance(CreateInstanceRequest request)
        {
            if (request == null || !request.IsValid())
                return InstanceResponse.Failure(request);

            string applicationId = request.GroupName.ApplicationId;
            string component = request.GroupName.Component;
            string version = request.GroupName.Version;

            // Check to make sure we have a record for this version.
            Application application = _data.Get<Application>(applicationId);
            if (application == null)
                return InstanceResponse.Failure(request);
            if (!application.HasVersion(component, version))
                return InstanceResponse.Failure(request);

            // Insert the new instance to the data module
            var instance = new StitchInstance
            {
                Id = null,
                StoreVersion = 0,
                Adaptor = request.Adaptor,
                ExecutableArguments = request.ExecutableArguments ?? "",
                ExecutableName = request.ExecutableName,
                GroupName = request.GroupName,
                LastHeartbeatReceived = 0,
                Name = request.Name
            };
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not save new stitch instance");
                return InstanceResponse.Failure(request);
            }

            // Perform the actual create logic in the Stitches module
            var createdInstance = _stitchRequests.CreateInstance(instance);
            if (createdInstance == null)
            {
                _log.LogError("Stitch instance {0} could not be created", instance.GroupName);
                _data.Delete<StitchInstance>(instance.Id);
                return InstanceResponse.Failure(request);
            }

            // Update the stitch record in the Data module, log, and return
            createdInstance = _data.Update<StitchInstance>(createdInstance.Id, i => i.DirectoryPath = createdInstance.DirectoryPath);
            _log.LogDebug("Stitch instance {0} Id={1} created", createdInstance.GroupName, createdInstance.Id);
            return InstanceResponse.Success(request, createdInstance);
        }

        public InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            if (instance == null)
                return InstanceResponse.Failure(request);

            var startedInstance = _stitchRequests.StartInstance(instance);
            if (!startedInstance.IsStartedOrRunning())
            {
                _notifier.StitchStarted(instance);
                _log.LogDebug("Stitch instance {0} Id={1} started", instance.GroupName, instance.Id);
            }

            // The Stitches module will update status in all cases, so always save
            _data.Save(startedInstance);
            return InstanceResponse.Create(request, startedInstance.IsStartedOrRunning(), startedInstance);
        }

        public InstanceResponse StopInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            if (instance == null)
                return InstanceResponse.Failure(request);

            var stoppedInstance = _stitchRequests.StopInstance(instance);
            if (stoppedInstance.State != InstanceStateType.Stopped)
            {
                _log.LogError("Stitch instance {0} Id={3} could not be stopped", instance.GroupName, instance.Id);
                return InstanceResponse.Failure(request);
            }

            _data.Save(stoppedInstance);
            _notifier.StitchStopped(instance);
            _log.LogDebug("Stitch instance {0} Id={3} stopped", instance.GroupName, instance.Id);
            return InstanceResponse.Success(request, stoppedInstance);
        }
    }
}
