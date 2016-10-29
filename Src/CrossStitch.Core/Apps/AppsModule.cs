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
        private readonly InstanceManager _instances;
        private readonly AppDataStorage _dataStorage;
        private RunningNode _node;

        public AppsModule(AppsConfiguration configuration, INetwork network)
        {
            _configuration = configuration;
            _instances = new InstanceManager(configuration, 
                new AppFileSystem(configuration),
                new AppDataStorage(),
                network);
            _instances.AppStarted += InstancesOnAppStarted;
        }

        private List<InstanceInformation> GetInstanceInformation(InstanceInformationRequest instanceInformationRequest)
        {
            return _instances.GetInstanceInformation();
        }

        private void InstancesOnAppStarted(object sender, AppStartedEventArgs appStartedEventArgs)
        {
            _messageBus.Publish("Started", new AppInstanceEvent {
                InstanceId = appStartedEventArgs.InstanceId,
                NodeId = _node.NodeId
            });
        }

        public string Name { get { return "Apps"; } }

        public void Start(RunningNode context)
        {
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Subscribe<InstanceInformationRequest, List<InstanceInformation>>(GetInstanceInformation);

            _node = context;
            var results = _instances.StartupActiveInstances();
            foreach (var result in results.Where(isr => isr.IsSuccess == false))
            {
                _messageBus.Publish(LogEvent.Error, new LogEvent {
                    Exception = result.Exception,
                    Message = "Instance " + result.InstanceId + " failed to start"
                });
            }
            foreach (var result in results.Where(isr => isr.IsSuccess == true))
            {
                _messageBus.Publish(AppInstanceEvent.StartedEventName, new AppInstanceEvent {
                    InstanceId = result.InstanceId,
                    NodeId = context.NodeId
                });
            }
        }

        public void Stop()
        {
            _instances.StopAll(false);
            _subscriptions.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            Stop();
            _instances.Dispose();
        }
    }
}
