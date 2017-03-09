using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Modules.Stitches.Versions;
using CrossStitch.Stitch.V1.Core;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesModule : IModule
    {
        private readonly StitchFileSystem _fileSystem;
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private StitchInstanceManager _stitchInstanceManager;

        private ModuleLog _log;

        public StitchesModule(StitchesConfiguration configuration)
        {
            _fileSystem = new StitchFileSystem(configuration, new DateTimeVersionManager());
        }

        public string Name => ModuleNames.Stitches;

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _log = new ModuleLog(_messageBus, Name);

            _subscriptions = new SubscriptionCollection(core.MessageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(l => l.OnDefaultChannel().Invoke(GetInstanceInformation));
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l.WithChannelName(PackageFileUploadRequest.ChannelUpload).Invoke(UploadPackageFile));

            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelCreateVerified).Invoke(CreateNewInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelStartVerified).Invoke(StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelStopVerified).Invoke(StopInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.ChannelSendHeartbeatVerified).Invoke(SendHeartbeat));

            _stitchInstanceManager = new StitchInstanceManager(core, _fileSystem);
            _stitchInstanceManager.StitchStateChange += StitchInstancesOnStitchStateChanged;
            _stitchInstanceManager.HeartbeatReceived += StitchInstanceManagerOnHeartbeatReceived;
            _stitchInstanceManager.LogsReceived += StitchInstanceManagerOnLogsReceived;
            _stitchInstanceManager.RequestResponseReceived += StitchInstanceManagerOnRequestResponseReceived;

            _log.LogDebug("Started");
        }

        public void Stop()
        {
            _stitchInstanceManager?.StopAll();
            _stitchInstanceManager?.Dispose();
            _stitchInstanceManager = null;
            _subscriptions?.Dispose();
            _subscriptions = null;

            _log.LogDebug("Stopped");
        }

        public void Dispose()
        {
            Stop();
        }

        private List<InstanceInformation> GetInstanceInformation(InstanceInformationRequest instanceInformationRequest)
        {
            return _stitchInstanceManager.GetInstanceInformation();
        }

        private void StitchInstancesOnStitchStateChanged(object sender, StitchProcessEventArgs e)
        {
            var channel = e.IsRunning ? StitchInstanceEvent.ChannelStarted : StitchInstanceEvent.ChannelStopped;
            _messageBus.Publish(channel, new StitchInstanceEvent
            {
                InstanceId = e.InstanceId
            });

            _log.LogInformation("Stitch instance {0} is {1}", e.InstanceId, e.IsRunning ? "started" : "stopped");
        }

        private void StitchInstanceManagerOnRequestResponseReceived(object sender, RequestResponseReceivedEventArgs e)
        {
            // TODO: How to report errors here?
        }

        private void StitchInstanceManagerOnLogsReceived(object sender, LogsReceivedEventArgs e)
        {
            // TODO: Should get the StitchInstance from the data store and enrich this message?
            foreach (var s in e.Logs)
                _log.LogInformation("Stitch Id={0} Mesage; {1}", e.StitchInstanceId, s);
        }

        private void StitchInstanceManagerOnHeartbeatReceived(object sender, HeartbeatSyncReceivedEventArgs e)
        {
            _messageBus.Publish(StitchInstanceEvent.ChannelSynced, new StitchInstanceEvent
            {
                InstanceId = e.StitchInstanceId,
                DataId = e.Id
            });
        }

        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            // Save the file and generate a unique Version name
            string version = _fileSystem.SavePackageToLibrary(request.ApplicationId, request.Component, request.Contents);
            return new PackageFileUploadResponse(true, version);
        }

        // Creates an unzipped copy of the executable for the Stitch, and any other resource
        // allocation. Call StartInstance to start the instance
        private InstanceResponse CreateNewInstance(InstanceRequest request)
        {
            // Unzip a copy of the version from the library into the running base
            var result = _fileSystem.UnzipLibraryPackageToRunningBase(request.Instance.Application, request.Instance.Component, request.Instance.Version, request.Instance.Id);
            return InstanceResponse.Create(request, result.Success, result.Path);
        }

        // Starts the instance. Must have been created with CreateNewInstance first
        private InstanceResponse StartInstance(InstanceRequest request)
        {
            var result = _stitchInstanceManager.Start(request.Instance);
            return InstanceResponse.Create(request, result.Success);
        }

        private InstanceResponse StopInstance(InstanceRequest request)
        {
            var stopResult = _stitchInstanceManager.Stop(request.Id);
            if (!stopResult.Success)
                return InstanceResponse.Failure(request);

            return InstanceResponse.Success(request);
        }

        private InstanceResponse SendHeartbeat(InstanceRequest arg)
        {
            var result = _stitchInstanceManager.SendHeartbeat(arg.DataId, arg.Instance);
            return InstanceResponse.Create(arg, result.Found && result.Success);
        }
    }
}
