using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messaging;
using CrossStitch.Core.Node.Messages;
using CrossStitch.Core.Timer;

namespace CrossStitch.Core.Node
{
    public class RunningNode : IDisposable
    {
        private readonly IMessageBus _messageBus;
        private readonly List<IModule> _modules;
        private readonly List<IModule> _managedModules;
        private readonly SubscriptionCollection _subscriptions;

        public RunningNode(NodeConfiguration nodeConfig, IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _subscriptions = new SubscriptionCollection(messageBus);
            _modules = new List<IModule>();
            _managedModules = new List<IModule>();

            _managedModules.Add(new MessageTimerModule(messageBus));

            _subscriptions.TimerSubscribe(6, t => _messageBus.Publish(NodeStatus.BroadcastEvent, GetStatus()));
            _subscriptions.Subscribe<NodeStatusRequest, NodeStatus>(r => GetStatus(r.NodeId));
        }

        public Guid NodeId { get; set; }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
        }

        public void Start()
        {
            foreach (var module in _managedModules)
                module.Start(this);
            foreach (var module in _modules)
                module.Start(this);
        }

        public void Stop()
        {
            foreach (var module in _modules)
                module.Stop();
            foreach (var module in _managedModules)
                module.Stop();
        }

        private NodeStatus GetStatus()
        {
            return new NodeStatus {
                AccessedTime = DateTime.Now,
                RunningModules = _modules.Select(m => m.Name).ToList()
            };
        }

        private NodeStatus GetStatus(Guid nodeId)
        {
            if (nodeId == NodeId)
                return GetStatus();

            // TODO: RunningNode needs to keep a list of statuses of all known nodes here for querying
            // TODO: RunningNode needs to broadcast its status to the backplane periodically (every minute?) so all nodes can have an up-to-date list of node statuses.
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            foreach (var module in _managedModules)
                module.Dispose();
            foreach (var module in _modules)
                module.Dispose();
        }
    }
}