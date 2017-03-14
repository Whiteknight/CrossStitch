using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Modules;
using CrossStitch.Core.Modules.Core;
using System;

namespace CrossStitch.Core
{
    public class CrossStitchCore : IDisposable
    {
        public NodeConfiguration Configuration { get; set; }
        public ModuleCollection Modules { get; }
        public CoreModule CoreModule { get; }

        private bool _started;
        public ModuleLog Log { get; }

        public CrossStitchCore(NodeConfiguration configuration = null)
        {
            Configuration = configuration ?? NodeConfiguration.GetDefault();
            // TODO: Should store the current NodeId in a Node file somewhere, so we can get the 
            // same ID on service restart.
            NodeId = Guid.NewGuid();
            MessageBus = new Acquaintance.MessageBus();

            CoreModule = new CoreModule(this, MessageBus);
            Modules = new ModuleCollection();

            Log = new ModuleLog(MessageBus, "Core");
        }

        public Guid NodeId { get; }

        public IMessageBus MessageBus { get; }

        public void AddModule(IModule module)
        {
            if (module.Name == ModuleNames.Core)
                throw new Exception("Cannot create a module with reserved name 'Core'");
            Modules.Add(module);
            if (_started)
            {
                Modules.Add(module);
                module.Start(this);
                MessageBus.Publish(CoreEvent.ChannelModuleAdded, new CoreEvent(module.Name));
                Log.LogInformation("New module added: {0}", module.Name);
            }
        }

        public void Start()
        {
            if (_started)
                throw new Exception("Core is already started");

            // TODO: Move this to the master node, where we can either query the live status of the
            // current node or query the last-known status of the requested node.
            // Also, be clear whether we are querying by NodeId or NetworkNodeId (the former is
            // more likely)
            //_subscriptions.Listen<NodeStatusRequest, NodeStatus>(l => l.OnDefaultChannel().Invoke(r => GetStatus(r.NodeId)));

            Modules.AddMissingModules(this);
            CoreModule.Start(this);
            Modules.StartAll(this);
            Modules.WarnOnMissingModules(Log);

            _started = true;
            MessageBus.Publish(CoreEvent.ChannelInitialized, new CoreEvent());

            // TODO: Report version? Other details?
            Log.LogInformation("Core initialized Id={0}", NodeId);
        }

        // TODO: Break stop down into two-phases. Pre-stop alerts all modules about shutdown and does logging.
        // Stop will wait for all modules to indicate readiness or, after a timeout, force shutdown.
        public void Stop()
        {
            Log.LogInformation("Core is shutting down");

            Modules.StopAll(this);
            CoreModule.Stop();

            _started = false;

            // TODO: Should we publish a Stop event? Is anybody listening at this point?
        }

        public void Dispose()
        {
            Stop();

            MessageBus.Dispose();
        }
    }
}