using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.Node.Messages;
using CrossStitch.Core.Timer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Node
{
    public class RunningNode : IDisposable
    {
        private readonly List<IModule> _modules;
        private readonly List<IModule> _managedModules;
        private SubscriptionCollection _subscriptions;

        public RunningNode(NodeConfiguration nodeConfig, IMessageBus messageBus)
        {
            MessageBus = messageBus;

            _modules = new List<IModule>();
            _managedModules = new List<IModule>
            {
                new MessageTimerModule(messageBus),
                new ApplicationCoordinator()
            };
        }

        public Guid NodeId { get; set; }

        public IMessageBus MessageBus { get; }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
        }

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(MessageBus);
            _subscriptions.TimerSubscribe(6, t => MessageBus.Publish(NodeStatus.BroadcastEvent, GetStatus()));
            _subscriptions.Listen<NodeStatusRequest, NodeStatus>(r => GetStatus(r.NodeId));

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

            _subscriptions.Dispose();
            _subscriptions = null;
        }

        private NodeStatus GetStatus()
        {
            return new NodeStatus
            {
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
            _subscriptions?.Dispose();
            foreach (var module in _managedModules)
                module.Dispose();
            foreach (var module in _modules)
                module.Dispose();
        }
    }
}