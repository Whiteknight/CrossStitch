using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Modules.Data;
using System.Collections.Generic;

namespace CrossStitch.Core.Messages
{
    public class NodeStatus : IDataEntity
    {
        public const string BroadcastEvent = "NodeStatusBroadcast";

        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }
        public string NetworkNodeId { get; set; }

        public List<string> RunningModules { get; set; }
        public List<string> Zones { get; set; }
        public List<InstanceInformation> Instances { get; set; }
    }
}