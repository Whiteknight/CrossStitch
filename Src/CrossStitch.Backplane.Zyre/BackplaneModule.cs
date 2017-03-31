using Acquaintance;
using CrossStitch.Core;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Modules;
using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Stitch.Events;
using System;
using System.IO;
using CrossStitch.Backplane.Zyre.Models;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;

namespace CrossStitch.Backplane.Zyre
{
    // TODO: BackplaneModule should be cleared of all Zyre-specific logic and moved to 
    // CrossStitch.Core. 
    public sealed class BackplaneModule : IModule
    {
        private readonly IClusterBackplane _backplane;
        private readonly IMessageBus _messageBus;
        private readonly ModuleLog _log;
        private readonly BackplaneConfiguration _configuration;

        private int _workerThreadId;
        private SubscriptionCollection _subscriptions;
        private Guid _nodeNetworkId;

        public BackplaneModule(CrossStitchCore core, IClusterBackplane backplane = null, BackplaneConfiguration configuration = null)
        {
            _messageBus = core.MessageBus;
            _log = new ModuleLog(_messageBus, Name);

            _configuration = configuration ?? BackplaneConfiguration.GetDefault();
            _backplane = backplane ?? new ZyreBackplane(core, _configuration);

            // Forward messages from the backplane to the IMessageBus
            _backplane.MessageReceived += MessageReceivedHandler;
            _backplane.ClusterMember += ClusterMemberHandler;
            _backplane.ZoneMember += ZoneMemberHandler;
        }

        public string Name => ModuleNames.Backplane;

        // TODO: Need a mechanism to change zones, once we startup. We should be able to add/remove
        // zone membership at runtime (and preferrably, store those in the data module so we can 
        // check that on startup and override the values in the config file)

        public void Start()
        {
            // Setup subscriptions
            _workerThreadId = _messageBus.ThreadPool.StartDedicatedWorker();
            _subscriptions = new SubscriptionCollection(_messageBus);

            _subscriptions.Subscribe<ClusterMessage>(s => s
                .WithChannelName(ClusterMessage.SendEventName)
                .Invoke(e => _backplane.Send(e))
                .OnThread(_workerThreadId));
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(BroadcastNetworkInformation));
            _subscriptions.Subscribe<FileTransferRequest>(b => b
                .OnDefaultChannel()
                .Invoke(m => _backplane.TransferPackageFile(m.GroupName, m.NetworkNodeId, m.FilePath, m.FileName, m.JobId, m.TaskId)));

            // TODO: Listen to requests to get current network id, zones, etc.
            // TODO: Request to get info on known peers/zones?
            // TODO: Uptime/connected stats?

            var context = _backplane.Start();
            _nodeNetworkId = context.NodeNetworkId;
            _log.LogInformation("Joined cluster with NetworkNodeId={0}", _nodeNetworkId);
        }

        public void Stop()
        {
            _backplane.Stop();

            _backplane.MessageReceived -= MessageReceivedHandler;
            _backplane.ClusterMember -= ClusterMemberHandler;
            _backplane.ZoneMember -= ZoneMemberHandler;

            _subscriptions.Dispose();
            _subscriptions = null;
            _messageBus.ThreadPool.StopDedicatedWorker(_workerThreadId);
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { "NetworkId", _nodeNetworkId.ToString() },
                { "Connected", "true" } // TODO: Actually determine this
            };
        }

        public void Dispose()
        {
            Stop();
        }

        private void BroadcastNetworkInformation(CoreEvent obj)
        {
            // Publish the network node id of this node, so other modules (especially Master) can have it
            _messageBus.Publish(BackplaneEvent.ChannelNetworkIdChanged, new BackplaneEvent { Data = _nodeNetworkId.ToString() });

            // Publish the list of zones that this node belongs to so other modules (Master) know.
            var zones = string.Join(",", _configuration.Zones.OrEmptyIfNull());
            _messageBus.Publish(BackplaneEvent.ChannelSetZones, new BackplaneEvent
            {
                Data = zones
            });
        }

        // TODO: These event types are very Zyre-specific. Come up with new event types which are
        // more agnostic to the backplane implementation.
        private void ZoneMemberHandler(object sender, PayloadEventArgs<ZoneMemberEvent> e)
        {
            if (e.Command == ZoneMemberEvent.JoiningEvent)
                _log.LogInformation("New member added to zone={0} NodeId={1}", e.Payload.Zone, e.Payload.NodeUuid);
            if (e.Command == ZoneMemberEvent.LeavingEvent)
                _log.LogInformation("Member node has left zone={0} NodeId={1}", e.Payload.Zone, e.Payload.NodeUuid);

            _messageBus?.Publish(e.Command, e.Payload);
        }

        private void ClusterMemberHandler(object sender, PayloadEventArgs<ClusterMemberEvent> e)
        {
            if (e.Command == ClusterMemberEvent.EnteringEvent)
                _log.LogInformation("New node added to cluster NodeId={0}", e.Payload.NetworkNodeId);
            if (e.Command == ClusterMemberEvent.ExitingEvent)
                _log.LogInformation("Node has left cluster NodeId={0}", e.Payload.NetworkNodeId);

            _messageBus.Publish(e.Command, e.Payload);
        }

        private void MessageReceivedHandler(object sender, PayloadEventArgs<ClusterMessage> e)
        {
            if (_messageBus == null || e?.Payload == null)
                return;

            string channel = e.Command;
            switch (e.Payload.Header.PayloadType)
            {
                case MessagePayloadType.Object:
                    PublishPayloadObjects(channel, e.Payload);
                    break;
                case MessagePayloadType.InternalObject:
                    HandleInternalPayloadObjects(e.Payload);
                    break;
                default:
                    _log.LogWarning("Received message of unhandled type: " + e.Payload.Header.PayloadType);
                    break;
            }
        }

        private void PublishPayloadObjects(string channel, ClusterMessage envelope)
        {
            if (envelope.PayloadObject == null)
                return;

            try
            {
                var message = new PayloadObjectDecoder().DecodePayloadObject(channel, envelope);

                _log.LogDebug("Received object message of type {0} from {1}", envelope.PayloadObject.GetType().Name, envelope.Header.FromNetworkId);
                _messageBus.PublishMessage(message);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error decoding PayloadObject of type {0}", envelope.PayloadObject.GetType().Name);
            }
        }

        private void HandleInternalPayloadObjects(ClusterMessage envelope)
        {
            if (envelope.PayloadObject == null)
                return;

            try
            {
                var internalObject = envelope.PayloadObject;
                var fileTransferRequest = internalObject as FileTransferEnvelope;
                if (fileTransferRequest != null)
                {
                    _log.LogDebug("Received file transfer request from {0}", envelope.Header.FromNetworkId);
                    ReceiveFileTransferRequest(envelope, fileTransferRequest);
                    return;
                }

                _log.LogWarning("Received internal object of unhandled type {0} from {1}", internalObject.GetType().FullName, envelope.Header.FromNetworkId);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error decoding internal object of type {0}", envelope.PayloadObject.GetType().Name);
            }
        }

        private void ReceiveFileTransferRequest(ClusterMessage envelope, FileTransferEnvelope request)
        {
            using (var stream = new MemoryStream(request.Contents))
            {
                // TODO: Get more sophisticated with chunking, restart/retry, checksums, etc
                var response = _messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(PackageFileUploadRequest.ChannelFromRemote, new PackageFileUploadRequest
                {
                    Contents = stream,
                    FileName = request.FileName,
                    GroupName = new StitchGroupName(request.GroupName),
                    LocalOnly = true
                });

                var outEnvelope = new ClusterMessageBuilder()
                    .FromNode()
                    .ToNode(envelope.Header.FromNetworkId)
                    .WithObjectPayload(new CommandReceipt
                    {
                        Success = response.IsSuccess,
                        ReplyToJobId = request.JobId,
                        ReplyToTaskId = request.TaskId
                    })
                    .Build();
                _backplane.Send(outEnvelope);
            }
        }
    }
}