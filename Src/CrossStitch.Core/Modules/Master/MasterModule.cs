using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using System;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using StitchDataMessage = CrossStitch.Core.Messages.StitchDataMessage;

namespace CrossStitch.Core.Modules.Master
{
    // TODO: Rename to RouterModule?
    // The Master module coordinates multipart-commands across the cluster.
    public class MasterModule : IModule
    {
        private readonly MasterService _service;
        private readonly NodeConfiguration _configuration;

        private DataHelperClient _data;
        private SubscriptionCollection _subscriptions;
        private IMessageBus _messageBus;
        private ModuleLog _log;
        

        public MasterModule(CrossStitchCore core, NodeConfiguration configuration)
        {
            _configuration = configuration;
            _log = new ModuleLog(core.MessageBus, Name);
            _data = new DataHelperClient(core.MessageBus);
            _service = new MasterService(core, _log, _data);
        }

        // TODO: We need to keep track of Backplane zones, so we can know to schedule certain
        // commands only on nodes of certain zones.

        // TODO: We need some kind of scoring metric for a node to report, which will take into 
        // account the number of processor cores and available RAM, and reduce by the number of
        // running stitches, so we can know which nodes to deploy stitches to.

        // TODO: We need to be storing, through the Data Module, state instance of all nodes in the
        // cluster, including the current running node.

        // TODO: We need a model class that can represent the node with status, along with an array
        // of application.component.version/StitchId running on that node. We can pass this model 
        // over the network and store in the data module for usage here.

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

        // TODO: When we convert an input command to a stream of output commands, we will need some
        // kind of a Job object that we can use to keep track of state. The status of the Job can be
        // queried externally, and the job can be polled regularly to find jobs which are running 
        // very late and need to be alerted about.
        // TODO: A Job should have an ability to be rolled-back, by issuing a sequence of inverse 
        // commands.

        // TODO: Method to lookup NodeId by NetworkNodeId and vice-versa

        public string Name => ModuleNames.Master;

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _log = new ModuleLog(core.MessageBus, Name);
            _subscriptions = new SubscriptionCollection(_messageBus);
            _data = new DataHelperClient(_messageBus);

            // Publish the status of the node every 60 seconds
            int timerTickMultiple = (_configuration.StatusBroadcastIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(timerTickMultiple, b => b
                .Invoke(t => GenerateAndPublishNodeStatus())
                .OnWorkerThread());
            _subscriptions.Listen<NodeStatusRequest, NodeStatus>(b => b
                .OnDefaultChannel()
                .Invoke(m => _service.GetExistingNodeStatus(m.NodeId)));
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(m => GenerateAndPublishNodeStatus()));

            _subscriptions.Subscribe<ObjectsReceivedEvent<NodeStatus>>(b => b
                .WithChannelName(ReceivedEvent.ReceivedEventName(NodeStatus.BroadcastEvent))
                .Invoke(m => _service.SaveNodeStatus(m.Object)));

            _subscriptions.Subscribe<StitchDataMessage>(b => b
                .OnDefaultChannel()
                .Invoke(EnrichStitchDataMessageWithAddress)
                .OnWorkerThread()
                .WithFilter(m => m.ToNodeId == Guid.Empty && string.IsNullOrEmpty(m.ToNetworkId)));

            //messageBus.Subscribe<MessageEnvelope>(s => s
            //    .WithChannelName(MessageEnvelope.SendEventName)
            //    .Invoke(ResolveAppInstanceNodeIdAndSend)
            //    .OnWorkerThread()
            //    .WithFilter(IsMessageAddressedToAppInstance)
            //);
        }

        public void Stop()
        {
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
                _messageBus.Publish(NodeStatus.BroadcastEvent, message);
        }

        private void EnrichStitchDataMessageWithAddress(StitchDataMessage message)
        {
            foreach (var outMessage in _service.EnrichStitchDataMessageWithAddress(message))
                _messageBus.PublishMessage(outMessage);
        }
    }
}