using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Modules;
using System;
using System.Linq;

namespace CrossStitch.Core
{
    public class CrossStitchCore : IDisposable, CrossStitch.Stitch.IRunningNodeContext
    {
        public NodeConfiguration Configuration { get; set; }
        private readonly ModuleCollection _modules;
        private SubscriptionCollection _subscriptions;
        private bool _started;
        public ModuleLog Log { get; }

        public CrossStitchCore(NodeConfiguration configuration)
        {
            Configuration = configuration;
            // TODO: Should store the current NodeId in a Node file somewhere, so we can get the 
            // same ID on service restart.
            NodeId = Guid.NewGuid();
            MessageBus = new Acquaintance.MessageBus();

            _modules = new ModuleCollection();

            Log = new ModuleLog(MessageBus, "Core");
        }

        public string NetworkNodeId { get; private set; }

        public Guid NodeId { get; }

        public IMessageBus MessageBus { get; }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
            if (_started)
            {
                _modules.Add(module);
                module.Start(this);
                MessageBus.Publish(CoreEvent.ChannelModuleAdded, new CoreEvent(module.Name));
                Log.LogInformation("New module added: {0}", module.Name);
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

            // TODO: Move this to the master node, where we can either query the live status of the
            // current node or query the last-known status of the requested node.
            // Also, be clear whether we are querying by NodeId or NetworkNodeId (the former is
            // more likely)
            //_subscriptions.Listen<NodeStatusRequest, NodeStatus>(l => l.OnDefaultChannel().Invoke(r => GetStatus(r.NodeId)));

            _subscriptions.Subscribe<BackplaneEvent>(b => b.WithChannelName(BackplaneEvent.ChannelNetworkIdChanged).Invoke(OnNetworkNodeIdChanged));

            _modules.AddMissingModules(this);
            _modules.StartAll(this);
            _modules.WarnOnMissingModules(Log);

            _started = true;
            MessageBus.Publish(CoreEvent.ChannelInitialized, new CoreEvent());

            // TODO: Report version? Other details?
            Log.LogInformation("Core initialized Id={0}", NodeId);
        }

        private void OnNetworkNodeIdChanged(BackplaneEvent backplaneEvent)
        {
            NetworkNodeId = backplaneEvent.Data;
            Log.LogInformation("Network Node ID set. NetworkId=" + NetworkNodeId);
        }

        // TODO: Break stop down into two-phases. Pre-stop alerts all modules about shutdown and does logging.
        // Stop will wait for all modules to indicate readiness or, after a timeout, force shutdown.
        public void Stop()
        {
            Log.LogInformation("Core is shutting down");

            _modules.StopAll(this);

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
                RunningModules = _modules.AddedModules.ToList()
            };
        }

        public void Dispose()
        {
            Stop();

            MessageBus.Dispose();
        }
    }
}