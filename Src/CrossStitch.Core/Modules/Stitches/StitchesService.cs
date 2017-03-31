using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch.ProcessV1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesService : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchInstanceManager _stitchInstanceManager;
        private readonly IModuleLog _log;
        private readonly IDataRepository _data;
        private readonly IStitchEventNotifier _notifier;
        private readonly CrossStitchCore _core;

        public StitchesService(CrossStitchCore core, IDataRepository data, StitchFileSystem fileSystem, StitchInstanceManager stitchInstanceManager, StitchEventObserver observer, IModuleLog log, IStitchEventNotifier notifier)
        {
            _fileSystem = fileSystem;
            _data = data;
            _core = core;
            _notifier = notifier;
            _log = log;

            _stitchInstanceManager = stitchInstanceManager;
            _stitchInstanceManager.StitchStateChange += observer.StitchInstancesOnStitchStateChanged;
            _stitchInstanceManager.HeartbeatReceived += observer.StitchInstanceManagerOnHeartbeatReceived;
            _stitchInstanceManager.LogsReceived += observer.StitchInstanceManagerOnLogsReceived;
            _stitchInstanceManager.RequestResponseReceived += observer.StitchInstanceManagerOnRequestResponseReceived;
            _stitchInstanceManager.DataMessageReceived += observer.StitchInstanceManagerOnDataMessageReceived;
        }

        public int NumberOfRunningStitches => _stitchInstanceManager.GetNumberOfRunningStitches();

        public List<InstanceInformation> GetInstanceInformation()
        {
            return _stitchInstanceManager.GetInstanceInformation();
        }

        // On Core Initialization, get all stitch instances from the data store and start them.
        public void StartRunningStitchesOnStartup()
        {
            _log.LogDebug("Starting startup stitches");
            var instances = _data.GetAll<StitchInstance>();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started))
            {
                var request = new InstanceRequest
                {
                    Id = instance.Id
                };
                // Best effort. The StartInstanceInternal method logs errors
                StartInstanceInternal(request, instance);
            }
            _log.LogDebug("Startup stitches started");
        }

        public PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request)
        {
            if (!request.IsValidLocalRequest())
                return new PackageFileUploadResponse(false, null, null);

            // Save the file and generate a unique Version name
            var result = _fileSystem.SavePackageToLibrary(request.GroupName.Application, request.GroupName.Component, request.Contents);
            var groupName = new StitchGroupName(request.GroupName.Application, request.GroupName.Component, result.Version);

            _log.LogDebug("Uploaded package file {0}", groupName);
            return new PackageFileUploadResponse(true, groupName, result.FilePath);
        }

        public PackageFileUploadResponse UploadStitchPackageFileFromRemote(PackageFileUploadRequest request)
        {
            if (!request.IsValidRemoteRequest())
                return new PackageFileUploadResponse(false, null, null);

            // Save the file and generate a unique Version name
            var result = _fileSystem.SavePackageToLibrary(request.GroupName.Application, request.GroupName.Component, request.GroupName.Version, request.Contents);

            _log.LogDebug("Uploaded package file {0}", request.GroupName);
            return new PackageFileUploadResponse(true, request.GroupName, result.FilePath);
        }

        // Creates an unzipped copy of the executable for the Stitch, and any other resource
        // allocation. Call StartInstance to start the instance
        public LocalCreateInstanceResponse CreateNewInstance(LocalCreateInstanceRequest request)
        {
            try
            {
                var response = new LocalCreateInstanceResponse();
                if (request == null || !request.IsValid() || request.NumberOfInstances <= 0)
                {
                    response.IsSuccess = false;
                    return response;
                }

                for (int i = 0; i < request.NumberOfInstances; i++)
                {
                    var instance = CreateSingleNewInstanceInternal(request);
                    if (instance != null)
                        response.CreatedIds.Add(instance.Id);
                }
                response.IsSuccess = response.CreatedIds.Count == request.NumberOfInstances;
                return response;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not create new stitch instance");
                return null;
            }
        }

        private StitchInstance CreateSingleNewInstanceInternal(LocalCreateInstanceRequest request)
        {
            // Insert the new instance to the data module
            var instance = new StitchInstanceMapper(_core.NodeId, _core.Name).Map(request);
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not save new stitch instance");
                return null;
            }

            if (request.Adaptor.RequiresPackageUnzip)
            {
                // Unzip a copy of the version from the library into the running base
                var result = _fileSystem.UnzipLibraryPackageToRunningBase(instance.GroupName, instance.Id);
                if (!result.Success)
                {
                    _log.LogError("Could not unzip library package for new stitch {0}", instance.GroupName);
                    return null;
                }
                // TODO: We should move this into a class specific to ProcessV1 types.
                instance.Adaptor.Parameters[Parameters.DirectoryPath] = result.Path;
            }

            _data.Save(instance);
            _log.LogInformation("Created stitch instance Id={0} GroupName={1}", instance.Id, request.GroupName);
            _notifier.StitchCreated(instance);

            // StitchInstanceManager auto-creates the necessary adaptor on Start. We don't need to do anything for it here.
            return instance;
        }

        // TODO: Ability to CloneTo a local instance to a remote node
        // TODO: Ability to CloneFrom a remote instance to the local node
        // This will require handling Clone requests through the master node
        public InstanceResponse CloneInstance(InstanceRequest request)
        {
            var instanceId = request.Id;

            // Get a copy of the instance data from the Data module
            var instance = _data.Get<StitchInstance>(instanceId);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, instance does not exist.", instanceId);
                return InstanceResponse.Failure(request);
            }

            // Update the model to be fresh
            instance.Id = null;
            instance.StoreVersion = 0;

            // Insert the fresh version to the Data module
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not clone instance {0}, data could not be saved.", instanceId);
                return InstanceResponse.Failure(request);
            }

            // Report success
            _log.LogInformation("Instance {0} cloned to {1}", instanceId, instance.Id);
            _notifier.StitchCreated(instance);
            return InstanceResponse.Success(request, instance);
        }

        // Starts the instance. Must have been created with CreateNewInstance first
        public InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            return StartInstanceInternal(request, instance);
        }

        public StitchResourceUsage GetInstanceResources(string instanceId)
        {
            return _stitchInstanceManager.GetInstanceResources(instanceId);
        }

        private InstanceResponse StartInstanceInternal(InstanceRequest request, StitchInstance instance)
        {
            if (instance == null)
                return InstanceResponse.Failure(request);

            var result = _stitchInstanceManager.Start(instance);
            _data.Save(instance);
            if (!result.Success)
            {
                _log.LogError(result.Exception, "Could not start stitch {0}", request.Id);
                return InstanceResponse.Create(request, result.Success, instance);
            }

            _log.LogInformation("Started stitch {0} Id={1}", result.StitchInstance.GroupName, result.StitchInstance.Id);
            _notifier.StitchStarted(instance);
            return InstanceResponse.Create(request, result.Success, instance);
        }

        public InstanceResponse StopInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            return StopInstanceInternal(request, instance);
        }

        private InstanceResponse StopInstanceInternal(InstanceRequest request, StitchInstance instance)
        {
            if (instance == null)
            {
                _log.LogError("Could not find stitch {0}", request.Id);
                return InstanceResponse.Failure(request);
            }

            var stopResult = _stitchInstanceManager.Stop(instance);
            _data.Save(stopResult.StitchInstance);

            if (!stopResult.Success || stopResult.StitchInstance.State != InstanceStateType.Stopped)
            {
                _log.LogError("Could not stop stitch {0}", request.Id);
                return InstanceResponse.Create(request, false);
            }

            _log.LogDebug("Stitch instance {0} Id={3} stopped", instance.GroupName, instance.Id);
            _notifier.StitchStopped(instance);

            return InstanceResponse.Create(request, true);
        }

        public InstanceResponse DeleteStitchInstance(InstanceRequest request)
        {
            bool success = true;
            var instanceId = request.Id;
            var instance = _data.Get<StitchInstance>(instanceId);

            // Tell the Stitches module to stop the Stitch
            var stopResponse = StopInstanceInternal(request, instance);
            if (!stopResponse.IsSuccess || instance.State != InstanceStateType.Stopped)
            {
                // We will continue to delete the record from the data store. When the node is restarted, this stitch
                // will not be brought back
                _log.LogError("Instance {0} could not be stopped. Deletion of records will continue.", instanceId);
                success = false;
            }

            // Delete the record from the Data module
            bool deleted = _data.Delete<StitchInstance>(instanceId);
            if (!deleted)
            {
                _log.LogError("Instance {0} could not be deleted", instanceId);
                success = false;
            }

            // Remove resources related to the stitch, including directories.
            var removeResult = _stitchInstanceManager.RemoveInstance(instance.Id);
            if (!removeResult.Success)
            {
                _log.LogError("Could not remove resources related to Stitch Id={0}", instance.Id);
                success = false;
            }

            _notifier.StitchDeleted(instanceId, instance.GroupName);

            _log.LogInformation("Instance {0} stopped and deleted successfully", instanceId);
            return InstanceResponse.Create(request, success, instance);
        }

        public void SendHeartbeat(long heartbeatId)
        {
            _stitchInstanceManager.SendHeartbeat(heartbeatId);
        }

        public void SendDataMessageToStitch(StitchDataMessage message)
        {
            var fullStitchId = message.GetRecipientId();
            if (!fullStitchId.IsLocalOnly && fullStitchId.NodeId != _core.NodeId)
                _log.LogWarning("Received message for stitch on the wrong node NodeId={0}", fullStitchId.NodeId);
            var result = _stitchInstanceManager.SendDataMessage(fullStitchId, message);
            if (result.Success)
                _log.LogDebug("Sent message Id={0} to StitchInstanceId={1}", message.Id, fullStitchId.StitchInstanceId);
            else
                _log.LogWarning("Could not deliver message Id={0} to StitchInstanceId={1}", message.Id, fullStitchId.StitchInstanceId);
        }

        public void StopAllOnShutdown()
        {
            // Stop all instances, we don't need to send out logs and notifications because the application is terminating
            // and subscribers will probably be gone.
            _stitchInstanceManager.StopAll();
            _log.LogDebug("Stopped");
        }

        public void Dispose()
        {
            _stitchInstanceManager.Dispose();
        }
    }
}
