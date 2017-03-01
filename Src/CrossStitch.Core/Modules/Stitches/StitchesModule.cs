using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Logging.Events;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Core.Modules.Stitches.Versions;
using CrossStitch.Core.Networking;
using CrossStitch.Core.Node;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesModule : IModule
    {
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private InstanceManager _instanceManager;
        private StitchesdataStorage _dataStorage;
        private RunningNode _node;
        private readonly INetwork _network;
        private readonly StitchFileSystem _fileSystem;
        private DataHelperClient _data;

        public StitchesModule(StitchesConfiguration configuration, INetwork network)
        {
            _network = network;
            _fileSystem = new StitchFileSystem(configuration, new DateTimeVersionManager());
        }

        public string Name => "Stitches";

        public void Start(RunningNode context)
        {
            _node = context;
            _messageBus = context.MessageBus;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(l => l.OnDefaultChannel().Invoke(GetInstanceInformation));
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l.OnDefaultChannel().Invoke(UploadPackageFile));

            _subscriptions.Listen<Instance, Instance>(l => l.WithChannelName(Instance.CreateEvent).Invoke(CreateInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Start).Invoke(StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l.WithChannelName(InstanceRequest.Stop).Invoke(StopInstance));

            _dataStorage = new StitchesdataStorage(context.MessageBus);
            _instanceManager = new InstanceManager(context, _fileSystem);
            _instanceManager.AppStarted += InstancesOnAppStarted;
            _data = new DataHelperClient(_messageBus);

            StartupInstances();
        }

        public void Stop()
        {
            _instanceManager?.StopAll();
            _instanceManager?.Dispose();
            _instanceManager = null;
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            Stop();
            _instanceManager.Dispose();
        }

        private void StartupInstances()
        {
            var instances = _dataStorage.GetAllInstances();
            var results = _instanceManager.StartupActiveInstances(instances);
            foreach (var result in results.Where(isr => !isr.Success))
            {
                if (result.Instance != null)
                    _dataStorage.Save(result.Instance);
                _messageBus.Publish(LogEvent.Error, new LogEvent
                {
                    Exception = result.Exception,
                    Message = "Instance " + result.InstanceId + " failed to start"
                });
            }
            foreach (var result in results.Where(isr => isr.Success))
            {
                // We don't need to save the Instance here, because we aren't changing its state/
                // It is still "Started"
                _messageBus.Publish(AppInstanceEvent.StartedEventName, new AppInstanceEvent
                {
                    InstanceId = result.InstanceId,
                    NodeId = _node.NodeId
                });
            }
        }

        private List<InstanceInformation> GetInstanceInformation(InstanceInformationRequest instanceInformationRequest)
        {
            return _instanceManager.GetInstanceInformation();
        }

        private void InstancesOnAppStarted(object sender, StitchProcessEventArgs stitchProcessEventArgs)
        {
            _messageBus.Publish("Started", new AppInstanceEvent
            {
                InstanceId = stitchProcessEventArgs.InstanceId,
                NodeId = _node.NodeId
            });
        }

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
            _data.Update<Application>(request.Application, a => a.AddVersion(request.Component, version));
            return new PackageFileUploadResponse(true, version);
        }

        private Instance CreateInstance(Instance instance)
        {
            // Check to make sure we have a record for this version.
            Application application = _dataStorage.GetApplication(instance.Application);
            if (application == null)
                return null;
            if (!application.HasVersion(instance.Component, instance.Version))
                return null;

            instance = _data.Insert(instance);

            // Unzip a copy of the version from the library into the running base
            var result = _fileSystem.UnzipLibraryPackageToRunningBase(instance.Application, instance.Component, instance.Version, instance.Id);

            // TODO: How do we communicate failure here? We need to use better request/response types for this
            if (!result.Success)
            {
                _data.Delete<Instance>(instance.Id);
                return null;
            }
            instance = _data.Update<Instance>(instance.Id, i => i.DirectoryPath = result.Path);

            return instance;
        }

        private InstanceResponse StartInstance(InstanceRequest request)
        {
            var instance = _data.Get<Instance>(request.Id);
            var result = _instanceManager.Start(instance);
            return new InstanceResponse
            {
                Success = result.Success
            };
        }

        private InstanceResponse StopInstance(InstanceRequest request)
        {
            var stopResult = _instanceManager.Stop(request.Id);
            if (!stopResult.Success)
                return new InstanceResponse { Success = false };

            var updateResult = _data.Update<Instance>(request.Id, instance => instance.State = InstanceStateType.Stopped);
            return new InstanceResponse { Success = updateResult != null };
        }
    }
}
