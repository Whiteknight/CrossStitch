using Acquaintance;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Master.Events;
using CrossStitch.Core.Utility.Serialization;
using System;

namespace ClusterTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(nodeConfig))
            {
                var serializer = new JsonSerializer();
                var backplaneConfig = BackplaneConfiguration.GetDefault();
                var backplane = new ZyreBackplane(backplaneConfig, "Client", serializer);
                var backplaneModule = new BackplaneModule(backplane);

                core.MessageBus.Subscribe<NodeAddedToClusterEvent>(l => l.WithChannelName(NodeAddedToClusterEvent.EventName).Invoke(NodeAdded));
                core.MessageBus.Subscribe<NodeRemovedFromClusterEvent>(l => l.WithChannelName(NodeRemovedFromClusterEvent.EventName).Invoke(NodeRemoved));

                core.AddModule(backplaneModule);
                core.AddModule(new LoggingModule(Common.Logging.LogManager.GetLogger("CrossStitch")));

                core.Start();
                core.Log.LogInformation("Starting CLIENT node {0}", core.NetworkNodeId);

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
