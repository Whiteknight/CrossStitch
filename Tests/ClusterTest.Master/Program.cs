using System;
using CrossStitch.App;
using CrossStitch.Core;
using CrossStitch.Core.Backplane;
using CrossStitch.Core.Master;
using CrossStitch.Core.Master.Events;
using CrossStitch.Core.Messaging;

namespace ClusterTest.Master
{
    class Program
    {
        static void Main(string[] args)
        {
            var masterConfig = MasterConfiguration.GetDefault();
            var serializer = new JsonSerializer();
            var messageBus = new LocalMessageBus();
            var backplaneConfig = BackplaneConfiguration.GetDefault();
            var backplane = new ZyreBackplane(backplaneConfig, "Master", serializer);
            var backplaneModule = new BackplaneModule(backplaneConfig, backplane, messageBus);
            var nodeManager = new ClusterNodeManager(messageBus);
            var masterModule = new MasterModule(backplane, nodeManager, messageBus);

            messageBus.Subscribe<NodeAddedToClusterEvent>(NodeAddedToClusterEvent.EventName, NodeAdded);
            messageBus.Subscribe<NodeRemovedFromClusterEvent>(NodeRemovedFromClusterEvent.EventName, NodeRemoved);
            //messageBus.Subscribe<ClusterCommandEvent>(ClusterCommandEvent.)

            var nodeConfig = NodeConfiguration.GetDefault();
            using (var node = new RunningNode(backplaneConfig, nodeConfig, messageBus))
            {
                node.AddModule(backplaneModule);
                node.AddModule(masterModule);
                
                node.Start();
                Console.WriteLine("Started MASTER node {0}", node.NodeId);

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
