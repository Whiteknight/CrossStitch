using System;
using CrossStitch.App;
using CrossStitch.Core;
using CrossStitch.Core.Backplane;
using CrossStitch.Core.Master.Events;
using CrossStitch.Core.Messaging;

namespace ClusterTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serializer = new JsonSerializer();
            var messageBus = new LocalMessageBus();
            var backplaneConfig = BackplaneConfiguration.GetDefault();
            var backplane = new ZyreBackplane(backplaneConfig, "Client", serializer);
            var backplaneModule = new BackplaneModule(backplaneConfig, backplane, messageBus);

            messageBus.Subscribe<NodeAddedToClusterEvent>(NodeAddedToClusterEvent.EventName, NodeAdded);
            messageBus.Subscribe<NodeRemovedFromClusterEvent>(NodeRemovedFromClusterEvent.EventName, NodeRemoved);

            var nodeConfig = NodeConfiguration.GetDefault();
            using (var node = new RunningNode(backplaneConfig, nodeConfig, messageBus))
            {
                node.AddModule(backplaneModule);
                node.Start();
                Console.WriteLine("Starting CLIENT node {0}", node.NodeId);

                Console.ReadKey();

                Console.WriteLine("Stopping node {0}", node.NodeId);
                node.Stop();
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
