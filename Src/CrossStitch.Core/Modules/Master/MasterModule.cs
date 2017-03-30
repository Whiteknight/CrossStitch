using Acquaintance;
using Acquaintance.Timers;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using System.Collections.Generic;

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

            // On startup, publish the node status and get info from the backplane
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(m => GenerateAndPublishNodeStatus()));
            _subscriptions.Subscribe<BackplaneEvent>(b => b
                .WithChannelName(BackplaneEvent.ChannelNetworkIdChanged)
                .Invoke(m => _service.SetNetworkNodeId(m.Data)));
            _subscriptions.Subscribe<BackplaneEvent>(b => b
                .WithChannelName(BackplaneEvent.ChannelSetZones)
                .Invoke(m => _service.SetClusterZones((m.Data ?? string.Empty).Split(','))));

            // Publish the status of the node every 60 seconds
            int timerTickMultiple = (_configuration.StatusBroadcastIntervalMinutes * 60) / Timer.MessageTimerModule.TimerIntervalSeconds;
            _subscriptions.TimerSubscribe(timerTickMultiple, b => b
                .Invoke(t => GenerateAndPublishNodeStatus())
                .OnWorkerThread());

            // TODO: Publish NodeStatus to cluster when Modules or StitchInstances change

            _subscriptions.Listen<StitchSummaryRequest, List<StitchSummary>>(b => b
                .OnDefaultChannel()
                .Invoke(_service.GetStitchSummaries));

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
                .Invoke(m => _data.StitchCache.RemoveLocalStitch(m.InstanceId))
                .OnThread(_cacheThreadId));

            // TODO: On Stitch Started/Stopped we should publish notification to the cluster so other Master nodes can update their
            // caches.

            // Upload package files and distribute to all nodes
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l
                .OnDefaultChannel()
                .Invoke(UploadPackageFile));
            // Create new stitch instances 
            _subscriptions.Listen<CreateInstanceRequest, CreateInstanceResponse>(l => l
                .OnDefaultChannel()
                .Invoke(_service.CreateNewInstances));
            _subscriptions.Subscribe<ObjectReceivedEvent<CreateInstanceRequest>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(m => _service.CreateNewInstanceFromRemote(m, m.Object)));

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
                .Invoke(m => ReceiveCommandJobReceipt(m, m.Object)));

            // Route StitchDataMessage to the correct node
            _subscriptions.Subscribe<StitchDataMessage>(b => b
                .OnDefaultChannel()
                .Invoke(_service.EnrichStitchDataMessageWithAddress));
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            // TODO: Return stats about the number of known peer nodes? Known Stitches? Usage stats?
            return new Dictionary<string, string>();
        }

        public void Dispose()
        {
            Stop();
        }

        private void GenerateAndPublishNodeStatus()
        {
            var message = _service.GenerateCurrentNodeStatus();
            if (message == null)
                return;

            // Save it to the data module, for quick lookup
            _data.Save(message, true);

            // Broadcast it locally, so any modules can get it if they want
            _messageBus.Publish(NodeStatus.BroadcastEvent, message);

            // Send it over the backplane, so all other nodes can be aware of it.
            var envelope = new ClusterMessageBuilder()
                .ToCluster()
                .FromNode()
                .WithObjectPayload(message)
                .Build();
            _messageBus.Publish(ClusterMessage.SendEventName, envelope);

            _log.LogDebug("Published updated node status");
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

        private PackageFileUploadResponse UploadPackageFile(PackageFileUploadRequest request)
        {
            var response = _messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(PackageFileUploadRequest.ChannelLocal, request);
            if (!response.Success)
            {
                _log.LogError("Could not upload package file");
                return response;
            }
            if (request.LocalOnly)
                return response;

            return _service.UploadStitchPackageFile(response.GroupName, response.FilePath, request);
        }

        private void ReceiveCommandJobReceipt(ReceivedEvent received, CommandReceipt receipt)
        {
            var eventMessage = _service.ReceiveReceiptFromRemote(received, receipt);
            if (eventMessage == null)
                return;

            _log.LogDebug("Job Id={0} is complete: {1}", eventMessage.JobId, eventMessage.Status);
            _messageBus.Publish(eventMessage.Status == JobStatusType.Success ? JobCompleteEvent.ChannelSuccess : JobCompleteEvent.ChannelFailure, eventMessage);
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

            public LocalCreateInstanceResponse CreateInstances(CreateInstanceRequest request, string networkNodeId, bool remote)
            {
                if (remote)
                {
                    var message = new ClusterMessageBuilder()
                        .FromNode()
                        .ToNode(networkNodeId)
                        .WithObjectPayload(request)
                        .Build();
                    _messageBus.Publish(ClusterMessage.SendEventName, message);
                    return null;
                }
                else
                    return _messageBus.Request<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(request);
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

            public void SendPackageFile(string networkNodeId, StitchGroupName groupName, string fileName, string filePath, string jobId, string taskId)
            {
                _messageBus.Publish(new FileTransferRequest
                {
                    FilePath = filePath,
                    NetworkNodeId = networkNodeId,
                    GroupName = groupName,
                    JobId = jobId,
                    TaskId = taskId,
                    FileName = fileName
                });
                //var message = new ClusterMessageBuilder()
                //    .FromNode()
                //    .ToNode(networkNodeId)
                //    .WithObjectPayload(new FileTransferRequest
                //    {
                //        FilePath = filePath,
                //        GroupName = groupName,
                //        JobId = jobId,
                //        TaskId = taskId,
                //        FileName = fileName
                //    })
                //    .Build();
                //_messageBus.Publish(ClusterMessage.SendEventName, message);
            }
        }
    }
}