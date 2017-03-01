using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.Modules.Timer;
using CrossStitch.Core.Node.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Node
{
    // TODO: We need to provide a smaller RunningNodeContext object which will hold things like NodeId 
    // and NodeName, but won't expose all the other methods from this class.
    public class RunningNode : IDisposable, CrossStitch.Stitch.IRunningNodeContext
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

        public string Name => NodeId.ToString();

        public IMessageBus MessageBus { get; }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
        }

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(MessageBus);

            // Publish the status of the node every 60 seconds
            _subscriptions.TimerSubscribe(6, b => b
                .Invoke(t => MessageBus.Publish(NodeStatus.BroadcastEvent, GetStatus()))
                .OnWorkerThread());
            _subscriptions.Listen<NodeStatusRequest, NodeStatus>(l => l.OnDefaultChannel().Invoke(r => GetStatus(r.NodeId)));

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

        public NodeStatus GetStatus()
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