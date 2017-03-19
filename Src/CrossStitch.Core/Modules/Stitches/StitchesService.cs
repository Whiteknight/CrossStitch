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
            _stitchInstanceManager = stitchInstanceManager;
            _log = log;

            _stitchInstanceManager.StitchStateChange += observer.StitchInstancesOnStitchStateChanged;
            _stitchInstanceManager.HeartbeatReceived += observer.StitchInstanceManagerOnHeartbeatReceived;
            _stitchInstanceManager.LogsReceived += observer.StitchInstanceManagerOnLogsReceived;
            _stitchInstanceManager.RequestResponseReceived += observer.StitchInstanceManagerOnRequestResponseReceived;
            _stitchInstanceManager.DataMessageReceived += observer.StitchInstanceManagerOnDataMessageReceived;
            _data = data;
            _core = core;
            _notifier = notifier;
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
                var response = StartInstanceInternal(request, instance);
            }
            _log.LogDebug("Startup stitches started");
        }

        public PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request)
        {
            if (!request.IsValid())
                return new PackageFileUploadResponse(false, null);

            // TODO: Validate the file. It should be a .zip
            // Save the file and generate a unique Version name
            string version = _fileSystem.SavePackageToLibrary(request.GroupName.Application, request.GroupName.Component, request.Contents);
            var groupName = new StitchGroupName(request.GroupName.Application, request.GroupName.Component, version);

            _log.LogDebug("Uploaded package file {0}", groupName);
            return new PackageFileUploadResponse(true, groupName);
        }

        // Creates an unzipped copy of the executable for the Stitch, and any other resource
        // allocation. Call StartInstance to start the instance
        public InstanceResponse CreateNewInstance(CreateInstanceRequest request)
        {
            if (request == null || !request.IsValid())
                return InstanceResponse.Failure(request);

            // Insert the new instance to the data module
            var instance = new StitchInstance
            {
                Id = null,
                StoreVersion = 0,
                Adaptor = request.Adaptor,
                GroupName = request.GroupName,
                LastHeartbeatReceived = 0,
                Name = request.Name,
                OwnerNodeName = _core.Name,
                OwnerNodeId = _core.NodeId
            };
            instance = _data.Insert(instance);
            if (instance == null)
            {
                _log.LogError("Could not save new stitch instance");
                return InstanceResponse.Failure(request);
            }

            // Unzip a copy of the version from the library into the running base
            var result = _fileSystem.UnzipLibraryPackageToRunningBase(instance.GroupName, instance.Id);
            if (!result.Success)
            {
                _log.LogError("Could not unzip library package for new stitch {0}", instance.GroupName);
                return InstanceResponse.Failure(request);
            }
            // TODO: We should move this into a class specific to ProcessV1 types.
            instance.Adaptor.Parameters[Parameters.DirectoryPath] = result.Path;
            var ok = _data.Save<StitchInstance>(instance);

            // StitchInstanceManager auto-creates the necessary adaptor on Start. We don't need to do anything for it here.
            return InstanceResponse.Success(request, instance);
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
            return InstanceResponse.Success(request, instance); ;
        }

        // Starts the instance. Must have been created with CreateNewInstance first
        public InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _data.Get<StitchInstance>(request.Id);
            return StartInstanceInternal(request, instance);
        }

        private InstanceResponse StartInstanceInternal(InstanceRequest request, StitchInstance instance)
        {
            if (instance == null)
                return InstanceResponse.Failure(request);

            var result = _stitchInstanceManager.Start(instance);
            if (result.Success)
            {
                _log.LogInformation("Started stitch {0} Id={1}", result.StitchInstance.GroupName, result.StitchInstance.Id);
                _notifier.StitchStarted(instance);
            }
            else
                _log.LogError("Could not start stitch {0}", request.Id);

            _data.Save(instance);
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
            if (stopResult.Success && stopResult.StitchInstance.State == InstanceStateType.Stopped)
            {
                _log.LogDebug("Stitch instance {0} Id={3} stopped", instance.GroupName, instance.Id);
                _notifier.StitchStopped(instance);
            }
            else
                _log.LogError("Could not stop stitch {0}", request.Id);

            _data.Save(stopResult.StitchInstance);
            return InstanceResponse.Create(request, stopResult.Success);
        }

        public InstanceResponse DeleteStitchInstance(InstanceRequest request)
        {
            var instanceId = request.Id;
            var instance = _data.Get<StitchInstance>(instanceId);

            // Tell the Stitches module to stop the Stitch
            var stopResponse = StopInstanceInternal(request, instance);
            if (!stopResponse.IsSuccess)
                return stopResponse;
            if (instance.State != InstanceStateType.Stopped)
            {
                _log.LogError("Instance {0} could not be stopped", instanceId);
                return InstanceResponse.Failure(request);
            }

            // Delete the record from the Data module
            bool deleted = _data.Delete<StitchInstance>(instanceId);
            if (!deleted)
            {
                _log.LogError("Instance {0} could not be deleted", instanceId);
                return InstanceResponse.Failure(request);
            }

            _log.LogInformation("Instance {0} stopped and deleted successfully", instanceId);

            return InstanceResponse.Success(request, instance);
        }

        public void SendHeartbeat(long heartbeatId)
        {
            _stitchInstanceManager.SendHeartbeat(heartbeatId);
        }

        public void SendDataMessageToStitch(StitchDataMessage message)
        {
            _stitchInstanceManager.SendDataMessage(message);
            _log.LogDebug("Sending message Id={0} to StitchInstanceId={1}", message.Id, message.ToStitchInstanceId);
        }

        public void StopAll()
        {
            _stitchInstanceManager.StopAll();
            _log.LogDebug("Stopped");
        }

        public void Dispose()
        {
            _stitchInstanceManager.Dispose();
        }
    }
}
