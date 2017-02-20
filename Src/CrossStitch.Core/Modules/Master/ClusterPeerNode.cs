using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Modules.Backplane;
using CrossStitch.Core.Modules.Stitches.Messages;

namespace CrossStitch.Core.Modules.Master
{
    public class ClusterPeerNode
    {
        public ClusterPeerNode(Guid nodeId, string name, NodeCommunicationInformation communications)
        {
            Name = name;
            NodeId = nodeId;
            Communications = communications;
        }

        public Guid NodeId { get; private set; }
        public string Name { get; private set; }
        public NodeCommunicationInformation Communications { get; private set; }

        public void SetActiveModules(IEnumerable<string> activeModules)
        {
            ActiveModules = activeModules.ToList();
        }
        public IEnumerable<string> ActiveModules { get; private set; }

        public void SetClusterZones(IEnumerable<string> zones)
        {
            ClusterZones = zones.ToList();
        }
        public IEnumerable<string> ClusterZones { get; private set; }

        public void SetClientApplicationInstances(IEnumerable<InstanceInformation> instances)
        {
            ClientApplicationInstances = instances.ToList();
        }
        public IEnumerable<InstanceInformation> ClientApplicationInstances { get; private set; }
    }
}