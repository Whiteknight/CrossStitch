using Acquaintance;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Master.Events;
using System;

namespace ClusterTest.Master
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(nodeConfig))
            {
                var backplane = new ZyreBackplane();
                var backplaneModule = new BackplaneModule(backplane);

                core.MessageBus.Subscribe<NodeAddedToClusterEvent>(s => s.WithChannelName(NodeAddedToClusterEvent.EventName).Invoke(NodeAdded));
                core.MessageBus.Subscribe<NodeRemovedFromClusterEvent>(s => s.WithChannelName(NodeRemovedFromClusterEvent.EventName).Invoke(NodeRemoved));

                core.AddModule(backplaneModule);
                core.AddModule(new LoggingModule(Common.Logging.LogManager.GetLogger("CrossStitch")));

                core.Start();
                core.Log.LogInformation("Started MASTER node {0}", core.NetworkNodeId);

                Console.ReadKey();

                core.Log.LogInformation("Stopping node {0}", core.NetworkNodeId);
                core.Stop();
            }
        }

        private static void NodeAdded(NodeAddedToClusterEvent e)
        {
            Console.WriteLine("Node {0} added to cluster Ip={1} port={2}", e.Node.NodeId, e.Node.Communications.Address, e.Node.Communications.ListenPort);
        }

        private static void NodeRemoved(NodeRemovedFromClusterEvent e)
        {
            Console.WriteLine("Node {0} removed from cluster Ip={1} port={2}", e.Node.NodeId, e.Node.Communications.Address, e.Node.Communications.ListenPort);
        }
    }
}
