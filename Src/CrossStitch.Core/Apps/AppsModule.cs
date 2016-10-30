using Acquaintance;
using CrossStitch.App.Networking;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Logging.Events;
using CrossStitch.Core.Node;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Apps
{
    public class AppsModule : IModule
    {
        private readonly AppsConfiguration _configuration;
        private IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;
        private InstanceManager _instanceManager;
        private AppsDataStorage _dataStorage;
        private RunningNode _node;
        private readonly INetwork _network;

        public AppsModule(AppsConfiguration configuration, INetwork network)
        {
            _configuration = configuration;
            _network = network;
        }

        public string Name { get { return "Apps"; } }

        public void Start(RunningNode context)
        {
            _node = context;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(GetInstanceInformation);

            _dataStorage = new AppsDataStorage(context.MessageBus);
            _instanceManager = new InstanceManager(_configuration, new AppFileSystem(_configuration), _dataStorage, _network);
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
    }
}
