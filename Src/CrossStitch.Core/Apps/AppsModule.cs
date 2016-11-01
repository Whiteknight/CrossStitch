using Acquaintance;
using CrossStitch.App.Networking;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Apps.Versions;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Logging.Events;
using CrossStitch.Core.Node;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Apps
{
    public class AppsModule : IModule
    {
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private InstanceManager _instanceManager;
        private AppsDataStorage _dataStorage;
        private RunningNode _node;
        private readonly INetwork _network;
        private readonly AppFileSystem _fileSystem;

        public AppsModule(AppsConfiguration configuration, INetwork network)
        {
            _network = network;
            _fileSystem = new AppFileSystem(configuration, new DateTimeVersionManager());
        }

        public string Name => "Apps";

        public void Start(RunningNode context)
        {
            _node = context;
            _messageBus = context.MessageBus;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(GetInstanceInformation);
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(UploadPackageFile);
            _subscriptions.Listen<Instance, Instance>(Instance.CreateEvent, CreateInstance);

            _dataStorage = new AppsDataStorage(context.MessageBus);
            _instanceManager = new InstanceManager(_fileSystem, _network);
            _instanceManager.AppStarted += InstancesOnAppStarted;

            StartupInstances();
        }

        public void Stop()
        {
            _instanceManager?.StopAll(false);
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
            foreach (var result in results.Where(isr => !isr.IsSuccess))
            {
                if (result.Instance != null)
                    _dataStorage.Save(result.Instance);
                _messageBus.Publish(LogEvent.Error, new LogEvent
                {
                    Exception = result.Exception,
                    Message = "Instance " + result.InstanceId + " failed to start"
                });
            }
            foreach (var result in results.Where(isr => isr.IsSuccess))
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

        private void InstancesOnAppStarted(object sender, AppStartedEventArgs appStartedEventArgs)
        {
            _messageBus.Publish("Started", new AppInstanceEvent
            {
                InstanceId = appStartedEventArgs.InstanceId,
                NodeId = _node.NodeId
            });
        }

        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            var application = _dataStorage.GetApplication(request.Application);
            if (application == null)
                return new PackageFileUploadResponse(false, null);
            if (!application.Components.Any(c => c.Name == request.Component))
                return new PackageFileUploadResponse(false, null);
            string version = _fileSystem.SavePackageToLibrary(request.Application, request.Component, request.Contents);
            return new PackageFileUploadResponse(true, version);
        }

        private Instance CreateInstance(Instance arg)
        {
            throw new NotImplementedException();
        }
    }
}
