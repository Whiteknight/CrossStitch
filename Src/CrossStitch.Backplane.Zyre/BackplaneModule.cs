using Acquaintance;
using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Core;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Modules;
using CrossStitch.Stitch.Events;
using System;

namespace CrossStitch.Backplane.Zyre
{
    // TODO: BackplaneModule should be cleared of all Zyre-specific logic and moved to 
    // CrossStitch.Core. 
    public sealed class BackplaneModule : IModule
    {
        private readonly IClusterBackplane _backplane;
        private IMessageBus _messageBus;
        private int _workerThreadId;
        private SubscriptionCollection _subscriptions;
        private Guid _nodeNetworkId;
        private ModuleLog _log;
        private MessageEnvelopeBuilderFactory _envelopeFactory;

        public BackplaneModule(IClusterBackplane backplane)
        {
            _backplane = backplane;
        }

        public string Name => ModuleNames.Backplane;

        // TODO: Need a mechanism to change zones, once we startup. We should be able to add/remove
        // zone membership at runtime (and preferrably, store those in the data module so we can 
        // check that on startup and override the values in the config file)

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _log = new ModuleLog(_messageBus, Name);

            _workerThreadId = _messageBus.ThreadPool.StartDedicatedWorker();
            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Subscribe<MessageEnvelope>(s => s
                .WithChannelName(MessageEnvelope.SendEventName)
                .Invoke(e => _backplane.Send(e))
                .OnThread(_workerThreadId)
                .WithFilter(IsMessageSendable));
            _subscriptions.Subscribe<NodeStatus>(s => s
                .WithChannelName(NodeStatus.BroadcastEvent)
                .Invoke(BroadcastNodeStatus));

            // Forward messages from the backplane to the IMessageBus
            _backplane.MessageReceived += MessageReceivedHandler;
            _backplane.ClusterMember += ClusterMemberHandler;
            _backplane.ZoneMember += ZoneMemberHandler;

            _nodeNetworkId = _backplane.Start(core);
            _envelopeFactory = new MessageEnvelopeBuilderFactory(_nodeNetworkId, core.NodeId);
            _log.LogInformation("Joined cluster with NetworkNodeId={0}", _nodeNetworkId);
            _messageBus.Publish(BackplaneEvent.ChannelNetworkIdChanged, new BackplaneEvent { Data = _nodeNetworkId.ToString() });
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

        public void Dispose()
        {
            Stop();
            _messageBus.ThreadPool.StopDedicatedWorker(_workerThreadId);
        }

        private void BroadcastNodeStatus(NodeStatus nodeStatus)
        {
            var envelope = _envelopeFactory.CreateNew()
                .ToCluster()
                .FromNode()
                .WithEventName(NodeStatus.BroadcastEvent)
                .WithObjectPayload(nodeStatus)
                .Build();
            _backplane.Send(envelope);
        }

        // TODO: These event types are very Zyre-specific. Come up with new event types which are
        // more agnostic to the backplane implementation.
        private void ZoneMemberHandler(object sender, PayloadEventArgs<ZoneMemberEvent> e)
        {
            _messageBus?.Publish(e);

            if (e.Command == ZoneMemberEvent.JoiningEvent)
                _log.LogInformation("New member added to zone={0} NodeId={1}", e.Payload.Zone, e.Payload.NodeUuid);
            if (e.Command == ZoneMemberEvent.LeavingEvent)
                _log.LogInformation("Member node has left zone={0} NodeId={1}", e.Payload.Zone, e.Payload.NodeUuid);
        }

        private void ClusterMemberHandler(object sender, PayloadEventArgs<ClusterMemberEvent> e)
        {
            _messageBus?.Publish(e);

            if (e.Command == ClusterMemberEvent.EnteringEvent)
                _log.LogInformation("New node added to cluster NodeId={0}", e.Payload.NodeUuid);
            if (e.Command == ClusterMemberEvent.ExitingEvent)
                _log.LogInformation("Node has left cluster NodeId={0}", e.Payload.NodeUuid);
        }

        private void MessageReceivedHandler(object sender, PayloadEventArgs<MessageEnvelope> e)
        {
            _messageBus?.Publish(e);
        }

        private static bool IsMessageSendable(MessageEnvelope envelope)
        {
            return envelope.Header.ToType == TargetType.Node ||
                   envelope.Header.ToType == TargetType.Zone ||
                   // TODO: Need to store the Proxy ID as a string, for consistency
                   // TODO: Need an extension method to get a Guid from the string.
                   (envelope.Header.ProxyNodeNetworkId.HasValue && envelope.Header.ProxyNodeNetworkId.Value != Guid.Empty);
        }
    }
}