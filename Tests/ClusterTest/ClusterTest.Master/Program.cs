using Acquaintance;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Master.Events;
using System;
using Acquaintance.Timers;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using Microsoft.Extensions.Logging;

namespace ClusterTest.Master
{
    class Program
    {
        private static string _remoteNodeId;

        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(nodeConfig))
            {
                core.MessageBus.Subscribe<NodeAddedToClusterEvent>(s => s.WithTopic(NodeAddedToClusterEvent.EventName).Invoke(NodeAdded));
                core.MessageBus.Subscribe<NodeRemovedFromClusterEvent>(s => s.WithTopic(NodeRemovedFromClusterEvent.EventName).Invoke(NodeRemoved));
                core.MessageBus.Subscribe<ObjectReceivedEvent<NodeStatus>>(b => b
                    .WithTopic(ReceivedEvent.ReceivedEventName(NodeStatus.BroadcastEvent))
                    .Invoke(ReceiveNodeStatus));
                core.MessageBus.TimerSubscribe("tick", 1, b => b.Invoke(m => SendPing(core)));

                core.AddModule(new BackplaneModule(core));
                var logger = new LoggerFactory().AddConsole(LogLevel.Debug).CreateLogger<Program>();
                core.AddModule(new LoggingModule(core, logger));

                core.Start();
                core.Log.LogInformation("Started MASTER node {0}", core.NodeId);

                Console.ReadKey();

                core.Log.LogInformation("Stopping node {0}", core.NodeId);
                core.Stop();
            }
        }

        private static void ReceiveNodeStatus(ObjectReceivedEvent<NodeStatus> m)
        {
            _remoteNodeId = m.FromNodeId;
        }

        private static void SendPing(CrossStitchCore core)
        {
            string remote = _remoteNodeId;
            if (string.IsNullOrEmpty(remote))
                return;

            var request = new CommandRequest
            {
                Command = CommandType.Ping,
                Target = _remoteNodeId
            };
            var response = core.MessageBus.RequestWait<CommandRequest, CommandResponse>(request);
            Console.WriteLine($"Sent ping Response={response.Result} JobId={response.ScheduledJobId}");
        }


        private static void NodeAdded(NodeAddedToClusterEvent e)
        {
            
            //Console.WriteLine("Node {0} added to cluster Ip={1} port={2}", e.Node.NodeId, e.Node.Communications.Address, e.Node.Communications.ListenPort);
        }

        private static void NodeRemoved(NodeRemovedFromClusterEvent e)
        {
            //Console.WriteLine("Node {0} removed from cluster Ip={1} port={2}", e.Node.NodeId, e.Node.Communications.Address, e.Node.Communications.ListenPort);
        }
    }
}
