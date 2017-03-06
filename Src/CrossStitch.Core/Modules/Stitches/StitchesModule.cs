using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Core.Modules.Stitches.Versions;
using CrossStitch.Core.Node;
using CrossStitch.Stitch.V1.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CrossStitch.Core.Modules.Stitches
{
    // TODO: Some kind of request/response to get the current heartbeat ID, so we can calculate
    // if a stitch instance is running behind.
    // TODO: Some kind of process to check the status of stitches, see how their LastHeartbeatId
    // compares to the current value, and send out alerts if things are looking unhealthy.
    public class StitchesModule : IModule
    {
        private readonly StitchesConfiguration _configuration;
        private readonly StitchFileSystem _fileSystem;

        // TODO: Move most of this mutable data into a state/context object
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private StitchInstanceManager _stitchInstanceManager;
        private StitchesDataStorage _dataStorage;
        private CrossStitchCore _node;
        private ModuleLog _log;
        private long _heartbeatId;

        public StitchesModule(StitchesConfiguration configuration)
        {
            _configuration = configuration;
            _fileSystem = new StitchFileSystem(configuration, new DateTimeVersionManager());
        }

        public string Name => "Stitches";

        public void Start(CrossStitchCore context)
        {
            _node = context;
            _messageBus = context.MessageBus;
            _log = new ModuleLog(_messageBus, Name);

            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(l => l.OnDefaultChannel().Invoke(GetInstanceInformation));
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l.OnDefaultChannel().Invoke(UploadPackageFile));

            _subscriptions.Listen<StitchInstance, StitchInstance>(l => l.WithChannelName(StitchInstance.CreateEvent).Invoke(CreateInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Start).Invoke(StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Stop).Invoke(StopInstance));

            //int timerTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / MessageTimerModule.TimerIntervalSeconds;
            int timerTickMultiple = 1;
            _subscriptions.TimerSubscribe(timerTickMultiple, b => b.Invoke(e => SendScheduledHeartbeat()));

            _dataStorage = new StitchesDataStorage(context.MessageBus);
            _stitchInstanceManager = new StitchInstanceManager(context, _fileSystem);
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
            _stitchInstanceManager.Dispose();
        }

        private List<InstanceInformation> GetInstanceInformation(InstanceInformationRequest instanceInformationRequest)
        {
            return _stitchInstanceManager.GetInstanceInformation();
        }

        private void StitchInstancesOnStitchStateChanged(object sender, StitchProcessEventArgs e)
        {
            _messageBus.Publish("Started", new AppInstanceEvent
            {
                InstanceId = e.InstanceId,
                NodeId = _node.NodeId
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
            _log.LogDebug("Stitch Id={0} Heartbeat sync received: {1}", e.StitchInstanceId, e.Id);
            _dataStorage.MarkHeartbeatSync(e.StitchInstanceId, e.Id);
        }

        // TODO: Move this into the ApplicationCoordinator
        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            // Get the application and make sure we have a Component record
            var application = _dataStorage.GetApplication(request.Application);
            if (application == null)
                return new PackageFileUploadResponse(false, null);
            if (application.Components.All(c => c.Name != request.Component))
                return new PackageFileUploadResponse(false, null);

            // Save the file and generate a unique Version name
            string version = _fileSystem.SavePackageToLibrary(request.Application, request.Component, request.Contents);

            // Update the Application record with the new Version
            _dataStorage.Update<Application>(request.Application, a => a.AddVersion(request.Component, version));
            _log.LogDebug("Uploaded package file {0}:{1}:{2}", request.Application, request.Component, version);
            return new PackageFileUploadResponse(true, version);
        }

        private StitchInstance CreateInstance(StitchInstance stitchInstance)
        {
            // Check to make sure we have a record for this version.
            Application application = _dataStorage.GetApplication(stitchInstance.Application);
            if (application == null)
                return null;
            if (!application.HasVersion(stitchInstance.Component, stitchInstance.Version))
                return null;

            stitchInstance = _dataStorage.Insert(stitchInstance);

            // Unzip a copy of the version from the library into the running base
            var result = _fileSystem.UnzipLibraryPackageToRunningBase(stitchInstance.Application, stitchInstance.Component, stitchInstance.Version, stitchInstance.Id);

            // TODO: How do we communicate failure here? We need to use better request/response types for this
            if (!result.Success)
            {
                _dataStorage.Delete<StitchInstance>(stitchInstance.Id);
                return null;
            }
            stitchInstance = _dataStorage.Update<StitchInstance>(stitchInstance.Id, i => i.DirectoryPath = result.Path);
            _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} created", stitchInstance.Application, stitchInstance.Component, stitchInstance.Version, stitchInstance.Id);
            return stitchInstance;
        }

        private InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _dataStorage.Get<StitchInstance>(request.Id);
            var result = _stitchInstanceManager.Start(instance);
            if (result.Success)
            {
                _messageBus.Publish(AppInstanceEvent.StartedEventName, new AppInstanceEvent
                {
                    InstanceId = result.InstanceId,
                    NodeId = _node.NodeId
                });
                _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} started", instance.Application, instance.Component, instance.Version, instance.Id);
            }
            else
            {
                _log.LogError(result.Exception, "Stitch instance {0}:{1}:{2} Id={3} failed to start", instance.Application, instance.Component, instance.Version, instance.Id);
            }
            return new InstanceResponse
            {
                Success = result.Success
            };
        }

        private InstanceResponse StopInstance(InstanceRequest request)
        {
            var instance = _dataStorage.GetInstance(request.Id);
            var stopResult = _stitchInstanceManager.Stop(request.Id);
            if (!stopResult.Success)
                return new InstanceResponse { Success = false };

            var updateResult = _dataStorage.Update<StitchInstance>(request.Id, i => i.State = InstanceStateType.Stopped);
            _log.LogDebug("Stitch instance {0}:{1}:{2} Id={3} stopped", instance.Application, instance.Component, instance.Version, instance.Id);
            return new InstanceResponse { Success = updateResult != null };
        }

        private void SendScheduledHeartbeat()
        {
            long id = Interlocked.Increment(ref _heartbeatId);
            _log.LogDebug("Sending heartbeat {0}", id);
            var instances = _dataStorage.GetAllInstances()
                .Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started)
                .ToList(); ;
            var results = _stitchInstanceManager.SendHeartbeats(id, instances);
            foreach (var result in results)
            {
                if (!result.Found)
                {
                    _dataStorage.Update<StitchInstance>(result.InstanceId, si => si.State = InstanceStateType.Missing);
                    continue;
                }
            }
        }
    }
}
