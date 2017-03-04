using Acquaintance;
using CrossStitch.Core.Modules.Backplane;
using CrossStitch.Core.Modules.Master;
using CrossStitch.Core.Modules.Master.Events;
using CrossStitch.Core.Node;
using CrossStitch.Core.Utility.Serialization;
using System;

namespace ClusterTest.Master
{
    class Program
    {
        static void Main(string[] args)
        {
            var masterConfig = MasterConfiguration.GetDefault();
            var serializer = new JsonSerializer();
            var messageBus = new MessageBus();
            var backplaneConfig = BackplaneConfiguration.GetDefault();
            var backplane = new ZyreBackplane(backplaneConfig, "Master", serializer);
            var backplaneModule = new BackplaneModule(backplane);
            var nodeManager = new ClusterNodeManager(messageBus);
            var masterModule = new MasterModule(nodeManager, messageBus);

            messageBus.Subscribe<NodeAddedToClusterEvent>(s => s.WithChannelName(NodeAddedToClusterEvent.EventName).Invoke(NodeAdded));
            messageBus.Subscribe<NodeRemovedFromClusterEvent>(s => s.WithChannelName(NodeRemovedFromClusterEvent.EventName).Invoke(NodeRemoved));
            //messageBus.Subscribe<ClusterCommandEvent>(ClusterCommandEvent.)

            var nodeConfig = NodeConfiguration.GetDefault();
            using (var node = new CrossStitchCore(nodeConfig, messageBus))
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
