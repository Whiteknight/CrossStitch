using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Master
{
    // The Master module keeps track of node/cluster state and routes messages to the appropriate
    // destinations
    public class MasterModule : IModule
    {
        private readonly MasterService _service;
        private readonly NodeConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly MasterDataRepository _data;
        private readonly ModuleLog _log;

        private int _cacheThreadId;
        private SubscriptionCollection _subscriptions;

        public MasterModule(CrossStitchCore core, NodeConfiguration configuration)
        {
            _configuration = configuration;
            _messageBus = core.MessageBus;
            _log = new ModuleLog(core.MessageBus, Name);
            var data = new DataHelperClient(core.MessageBus);
            _data = new MasterDataRepository(core.NodeId, data);
            var stitches = new StitchRequestHandler(core.MessageBus);
            var sender = new ClusterMessageSender(core.MessageBus);
            _service = new MasterService(core, _log, _data, stitches, sender);
        }

        // TODO: We need to keep track of Backplane zones, so we can know to schedule certain
        // commands only on nodes of certain zones.

        // TODO: We need some kind of scoring metric for a node to report, which will take into 
        // account the number of processor cores and available RAM, and reduce by the number of
        // running stitches, so we can know which nodes to deploy stitches to.

        /* Commands to support:
         * 1) Create N instances of a Stitch version, with automatic balancing across the cluster to nodes with space
         * 2) Move a stitch instance from current Node to specified remote Node
         * 3) Move a stitch instance from Specified remote node to current node
         * 4) Rebalance instances, by moving stitch instances from an overloaded node to an underloaded node
         * 5) Shutdown stitch instances of a particular version, if A instances are needed but B are running in the cluster and B>A
         * 6) Shutdown all stitch instances of any version besides the current version V, on all nodes.
         * 7) Create A-B stitch instances if A instances are needed but B are running in the cluster and A>B
         * 8) Deploy an application, by Deploying Ni instances of each Stitch/Component version I in an application manifest
         *      The manifest will include specific versions for each component and a number of instances to run for each component version.
         *      
         * Whether we want to represent these things by command objects or by some kind of parsible command script is to be determined.
         */

        // TODO: A CommandJob should have an ability to be rolled-back, by issuing a sequence of inverse 
        // commands.

        // TODO: Method to lookup NodeId by NetworkNodeId and vice-versa. Maybe maintain a cache
        // here instead of looking it up in the DataModule each time?

        public string Name => ModuleNames.Master;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);
            _cacheThreadId = _messageBus.ThreadPool.StartDedicatedWorker();

            // On startup, publish the node status
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(m => GenerateAndPublishNodeStatus()));

            // Publish the status of the node every 60 seconds
            int timerTickMultiple = (_configuration.StatusBroadcastIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(timerTickMultiple, b => b
                .Invoke(t => GenerateAndPublishNodeStatus())
                .OnWorkerThread());

            // Respond to requests for node status
            // TODO: Publish NodeStatus to cluster when Modules or StitchInstances change
            _subscriptions.Listen<NodeStatusRequest, NodeStatus>(b => b
                .OnDefaultChannel()
                .Invoke(m => _service.GetExistingNodeStatus(m.NodeId)));
            _subscriptions.Subscribe<ClusterMemberEvent>(b => b
                .WithChannelName(ClusterMemberEvent.EnteringEvent)
                .Invoke(SendNodeStatusToNewClusterNode));

            // Save node status from other nodes
            _subscriptions.Subscribe<ObjectReceivedEvent<NodeStatus>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(m => _service.SaveNodeStatus(m.Object)));

            // Subscribe to events for caching stitch status
            _subscriptions.Subscribe<ObjectReceivedEvent<NodeStatus>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(m => _data.StitchCache.AddNodeStatus(m, m.Object))
                .OnThread(_cacheThreadId));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelStarted)
                .Invoke(m => _data.StitchCache.AddLocalStitch(m.InstanceId, m.GroupName))
                .OnThread(_cacheThreadId));
            _subscriptions.Subscribe<StitchInstanceEvent>(b => b
                .WithChannelName(StitchInstanceEvent.ChannelStopped)
                .Invoke(m => _data.StitchCache.AddLocalStitch(m.InstanceId, m.GroupName))
                .OnThread(_cacheThreadId));

            // TODO: On Stitch Started/Stopped we should publish notification to the cluster so other Master nodes can update their
            // caches.

            // Handle incoming commands
            _subscriptions.Listen<CommandRequest, CommandResponse>(b => b
                .OnDefaultChannel()
                .Invoke(_service.DispatchCommandRequest));
            _subscriptions.Subscribe<ObjectReceivedEvent<CommandRequest>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(ore => _service.ReceiveCommandFromRemote(ore, ore.Object)));

            // Handle incoming command receipt messages
            _subscriptions.Subscribe<ObjectReceivedEvent<CommandReceipt>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(ore => _service.ReceiveReceiptFromRemote(ore, ore.Object)));

            // Route StitchDataMessage to the correct node
            _subscriptions.Subscribe<StitchDataMessage>(b => b
                .OnDefaultChannel()
                .Invoke(_service.EnrichStitchDataMessageWithAddress));
        }

        public void Stop()
        {
            _subscriptions.Dispose();
            _subscriptions = null;
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>();
        }

        public void Dispose()
        {
            Stop();
        }

        private void GenerateAndPublishNodeStatus()
        {
            var message = _service.GenerateCurrentNodeStatus();
            if (message != null)
            {
                _data.Save(message, true);
                _messageBus.Publish(NodeStatus.BroadcastEvent, message);
                _log.LogDebug("Published node status to cluster");
            }
        }

        private void SendNodeStatusToNewClusterNode(ClusterMemberEvent obj)
        {
            var message = _service.GenerateCurrentNodeStatus();
            if (message == null)
                return;

            var envelope = new ClusterMessageBuilder()
                .FromNode()
                .ToNode(obj.NetworkNodeId)
                .WithObjectPayload(message)
                .Build();
            _messageBus.Publish(ClusterMessage.SendEventName, envelope);
            _log.LogDebug("Published node status to node Id={0}", obj.NodeId);
        }

        private class StitchRequestHandler : IStitchRequestHandler
        {
            private readonly IMessageBus _messageBus;

            public StitchRequestHandler(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public bool StartInstance(string instanceId)
            {
                var request = new InstanceRequest
                {
                    Id = instanceId
                };
                var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, request);
                return response.IsSuccess;
            }

            public bool StopInstance(string instanceId)
            {
                var request = new InstanceRequest
                {
                    Id = instanceId
                };
                var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStop, request);
                return response.IsSuccess;
            }

            public bool RemoveInstance(string instanceId)
            {
                var request = new InstanceRequest
                {
                    Id = instanceId
                };
                var response = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelDelete, request);
                return response.IsSuccess;
            }

            public void SendStitchData(StitchDataMessage message, bool remote)
            {
                if (remote)
                {
                    var envelope = new ClusterMessageBuilder()
                        .ToNode(message.ToNetworkId)
                        .FromNode()
                        .WithObjectPayload(message)
                        .Build();
                    _messageBus.Publish(ClusterMessage.SendEventName, envelope);
                }
                else
                    _messageBus.Publish(StitchDataMessage.ChannelSendLocal, message);
            }
        }

        private class ClusterMessageSender : IClusterMessageSender
        {
            private readonly IMessageBus _messageBus;

            public ClusterMessageSender(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void Send(ClusterMessage message)
            {
                _messageBus.Publish(ClusterMessage.SendEventName, message);
            }

            public void SendReceipt(bool success, string networkNodeId, string jobId, string taskId)
            {
                var message = new ClusterMessageBuilder()
                    .FromNode()
                    .ToNode(networkNodeId)
                    .WithObjectPayload(new CommandReceipt
                    {
                        Success = success,
                        ReplyToJobId = jobId,
                        ReplyToTaskId = taskId
                    })
                    .Build();
                _messageBus.Publish(ClusterMessage.SendEventName, message);
            }
        }
    }
}