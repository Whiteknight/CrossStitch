using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Apps;
using CrossStitch.Core.Communications;
using CrossStitch.Core.Messaging;

namespace CrossStitch.Core
{
    public class RunningNode : IDisposable
    {
        private readonly IMessageBus _messageBus;
        private readonly List<IModule> _modules;

        public RunningNode(BackplaneConfiguration backplaneConfig, NodeConfiguration nodeConfig, IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _modules = new List<IModule>();
            // TODO: Persist this somewhere, and read it out if we restart?
            Communications = new NodeCommunicationInformation {
                Address = "127.0.0.1",
                ListenPort = backplaneConfig == null ? 0 : backplaneConfig.ListenPort
            };
            ClientApplicationInstances = Enumerable.Empty<InstanceInformation>();
        }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
        }

        public void Start()
        {
            foreach (var module in _modules)
                module.Start(this);
        }

        public void Stop()
        {
            foreach (var module in _modules)
                module.Stop();
        }

        public Guid NodeId { get;  set; }

        public NodeCommunicationInformation Communications { get; private set; }

        public IEnumerable<string> ActiveModules { get { return _modules.Select(m => m.Name); } }

        public IEnumerable<InstanceInformation> ClientApplicationInstances { get; private set; }
        public IReadOnlyList<IModule> Modules { get; private set; }
        public void Dispose()
        {
            foreach (var module in _modules)
                module.Dispose();
        }
    }
}