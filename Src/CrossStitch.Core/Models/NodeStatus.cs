using CrossStitch.Core.Messages.Stitches;
using System.Collections.Generic;
using CrossStitch.Core.Messages.Backplane;

namespace CrossStitch.Core.Models
{
    public class NodeStatus : IDataEntity, IRequiresNetworkNodeId
    {
        public const string BroadcastEvent = "NodeStatusBroadcast";

        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }

        public string NetworkNodeId { get; set; }

        public List<string> RunningModules { get; set; }
        public List<string> Zones { get; set; }
        public List<InstanceInformation> StitchInstances { get; set; }
    }
}