using Acquaintance;
using CrossStitch.Core.Modules.Backplane;
using CrossStitch.Core.Modules.Master.Events;
using CrossStitch.Core.Node;
using CrossStitch.Core.Utility.Serialization;
using System;

namespace ClusterTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serializer = new JsonSerializer();
            var backplaneConfig = BackplaneConfiguration.GetDefault();
            var backplane = new ZyreBackplane(backplaneConfig, "Client", serializer);
            var backplaneModule = new BackplaneModule(backplane);

            var nodeConfig = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(nodeConfig))
            {
                core.MessageBus.Subscribe<NodeAddedToClusterEvent>(l => l.WithChannelName(NodeAddedToClusterEvent.EventName).Invoke(NodeAdded));
                core.MessageBus.Subscribe<NodeRemovedFromClusterEvent>(l => l.WithChannelName(NodeRemovedFromClusterEvent.EventName).Invoke(NodeRemoved));

                core.AddModule(backplaneModule);
                core.Start();
                Console.WriteLine("Starting CLIENT node {0}", core.NodeId);

                Console.ReadKey();

                Console.WriteLine("Stopping node {0}", core.NodeId);
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
