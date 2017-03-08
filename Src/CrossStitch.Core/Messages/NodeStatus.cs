using CrossStitch.Core.Modules.Stitches.Messages;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Messages
{
    public class NodeStatus
    {
        public const string BroadcastEvent = "NodeStatusBroadcast";
        public DateTime AccessedTime { get; set; }
        public List<string> RunningModules { get; set; }
        public List<InstanceInformation> Instances { get; set; }
    }
}