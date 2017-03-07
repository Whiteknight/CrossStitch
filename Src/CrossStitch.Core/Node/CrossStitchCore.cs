using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules;
using CrossStitch.Core.Modules.Timer;
using CrossStitch.Core.Node.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Node
{
    public class CrossStitchCore : IDisposable, CrossStitch.Stitch.IRunningNodeContext
    {
        private readonly List<IModule> _modules;
        private readonly List<IModule> _managedModules;
        private SubscriptionCollection _subscriptions;
        private bool _started;
        private readonly ModuleLog _log;

        public CrossStitchCore(NodeConfiguration nodeConfig)
        {
            MessageBus = new Acquaintance.MessageBus();

            _modules = new List<IModule>();
            _managedModules = new List<IModule>
            {
                new MessageTimerModule(MessageBus),
                new ApplicationCoordinatorModule()
            };
            _log = new ModuleLog(MessageBus, "Core");
        }

        private Guid _nodeId;
        public Guid NodeId
        {
            get { return _nodeId; }
            set
            {
                string oldName = Name;
                _nodeId = value;
                OnNodeIdChanged(Name, oldName);
            }
        }

        private void OnNodeIdChanged(string newName, string oldName)
        {
            if (_started)
                MessageBus.Publish(CoreEvent.ChannelNameChanged, new CoreEvent(newName, oldName));
        }

        public string Name => NodeId.ToString();

        public IMessageBus MessageBus { get; }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
            if (_started)
            {
                module.Start(this);
                MessageBus.Publish(CoreEvent.ChannelModuleAdded, new CoreEvent(Name, module.Name));
                _log.LogInformation("New module added: {0}", module.Name);
            }
        }

        public void Start()
        {
            if (_started)
                throw new Exception("Core is already started");

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

            _started = true;
            MessageBus.Publish(CoreEvent.ChannelInitialized, new CoreEvent(Name));
            _log.LogError("Core initialized");
        }

        // TODO: Break stop down into two-phases. Pre-stop alerts all modules about shutdown and does logging.
        // Stop will wait for all modules to indicate readiness or, after a timeout, force shutdown.
        public void Stop()
        {
            _log.LogInformation("Core is shutting down");

            foreach (var module in _modules)
                module.Stop();
            foreach (var module in _managedModules)
                module.Stop();

            _subscriptions.Dispose();
            _subscriptions = null;

            _started = false;

            // TODO: Should we publish a Stop event? Is anybody listening at this point?
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