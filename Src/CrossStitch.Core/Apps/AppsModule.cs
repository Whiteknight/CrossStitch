using System.Collections.Generic;
using System.Linq;
using CrossStitch.App.Networking;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Logging.Events;
using Acquaintance;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Apps
{
    public class AppsModule : IModule
    {
        private readonly AppsConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly InstanceManager _instances;
        private readonly AppDataStorage _dataStorage;
        private RunningNode _node;

        public AppsModule(AppsConfiguration configuration, IMessageBus messageBus, INetwork network)
        {
            _configuration = configuration;
            _messageBus = messageBus;
            _instances = new InstanceManager(configuration, 
                new AppFileSystem(configuration),
                new AppDataStorage(),
                network);
            _instances.AppStarted += InstancesOnAppStarted;

            _messageBus.Subscribe<InstanceInformationRequest, List<InstanceInformation>>(GetInstanceInformation);
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
        }

        public void Dispose()
        {
            Stop();
            _instances.Dispose();
        }
    }
}
