using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Utility;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesService : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchInstanceManager _stitchInstanceManager;
        private readonly IModuleLog _log;

        public StitchesService(StitchFileSystem fileSystem, StitchInstanceManager stitchInstanceManager, StitchEventObserver observer, IModuleLog log)
        {
            _fileSystem = fileSystem;
            _stitchInstanceManager = stitchInstanceManager;
            _log = log;

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

        public PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            // Save the file and generate a unique Version name
            string version = _fileSystem.SavePackageToLibrary(request.ApplicationId, request.Component, request.Contents);
            return new PackageFileUploadResponse(true, version);
        }

        // Creates an unzipped copy of the executable for the Stitch, and any other resource
        // allocation. Call StartInstance to start the instance
        public InstanceResponse CreateNewInstance(EnrichedInstanceRequest request)
        {
            // Unzip a copy of the version from the library into the running base
            var result = _fileSystem.UnzipLibraryPackageToRunningBase(request.StitchInstance.GroupName, request.StitchInstance.Id);
            if (!result.Success)
            {
                _log.LogError("Could not unzip library package for new stitch {0}", request.StitchInstance.GroupName);
                return InstanceResponse.Failure(request);
            }

            // StitchInstanceManager auto-creates the necessary adaptor on Start. We don't need to do anything for it here.
            return InstanceResponse.Create(request, result.Success, result.Path);
        }

        // Starts the instance. Must have been created with CreateNewInstance first
        public InstanceResponse StartInstance(EnrichedInstanceRequest request)
        {
            var result = _stitchInstanceManager.Start(request.StitchInstance);
            if (result.Success)
                _log.LogInformation("Starting stitch {0} Id={1}", result.StitchInstance.GroupName, result.StitchInstance.Id);
            else
                _log.LogError("Could not start stitch {0}", request.Id);

            return InstanceResponse.Create(request, result.Success);
        }

        public InstanceResponse StopInstance(EnrichedInstanceRequest request)
        {
            var stopResult = _stitchInstanceManager.Stop(request.StitchInstance);
            return InstanceResponse.Create(request, stopResult.Success);
        }

        public InstanceResponse SendHeartbeat(EnrichedInstanceRequest request)
        {
            var result = _stitchInstanceManager.SendHeartbeat(request.DataId, request.StitchInstance);
            return InstanceResponse.Create(request, result.Found && result.Success);
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
