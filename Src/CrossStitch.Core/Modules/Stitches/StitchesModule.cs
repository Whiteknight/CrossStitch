using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Core.Modules.Stitches.Versions;
using CrossStitch.Core.Modules.Timer;
using CrossStitch.Core.Node;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
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

            int timerTickMultiple = (_configuration.HeartbeatIntervalMinutes * 60) / MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(timerTickMultiple, b => b.Invoke(e => SendScheduledHeartbeat()));

            _dataStorage = new StitchesDataStorage(context.MessageBus);
            _stitchInstanceManager = new StitchInstanceManager(context, _fileSystem);
            _stitchInstanceManager.StitchStarted += StitchInstancesOnStitchStarted;

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

        private void StitchInstancesOnStitchStarted(object sender, StitchProcessEventArgs stitchProcessEventArgs)
        {
            _messageBus.Publish("Started", new AppInstanceEvent
            {
                InstanceId = stitchProcessEventArgs.InstanceId,
                NodeId = _node.NodeId
            });
            _log.LogInformation("Stitch instance {0} is started", stitchProcessEventArgs.InstanceId);
        }

        // TODO: Move this into the ApplicationCoordinator
        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            // Get the application and make sure we have a Component record
            var application = _dataStorage.GetApplication(request.Application);
            if (application == null)
                return new PackageFileUploadResponse(false, null);
            if (!application.Components.Any(c => c.Name == request.Component))
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
                if (result.Success)
                {
                    _dataStorage.MarkHeartbeatSync(result.InstanceId);
                    _log.LogDebug("Heartbeat sync received Id={0}", result.InstanceId);
                }
                else
                {
                    _dataStorage.MarkHeartbeatMissed(result.InstanceId);
                    _log.LogWarning("Heartbeat missed Id={0}", result.InstanceId);
                }
            }
        }
    }
}
