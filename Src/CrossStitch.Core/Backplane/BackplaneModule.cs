using Acquaintance;
using CrossStitch.App.Events;
using CrossStitch.App.Networking;
using CrossStitch.Core.Backplane.Events;
using CrossStitch.Core.Node;
using CrossStitch.Core.Node.Messages;
using System;

namespace CrossStitch.Core.Backplane
{
    public sealed class BackplaneModule : IModule
    {
        private readonly BackplaneConfiguration _configuration;
        private readonly IClusterBackplane _backplane;
        private IMessageBus _messageBus;
        private int _workerThreadId;
        private SubscriptionCollection _subscriptions;
        private Guid _nodeId;

        public BackplaneModule(BackplaneConfiguration configuration, IClusterBackplane backplane)
        {
            _configuration = configuration;
            _backplane = backplane;
        }

        private void BroadcastNodeStatus(NodeStatus nodeStatus)
        {
            var envelope = MessageEnvelope.CreateNew()
                .ToCluster()
                .FromNode(_nodeId)
                .WithEventName(NodeStatus.BroadcastEvent)
                .WithObjectPayload(nodeStatus)
                .Envelope;
            _backplane.Send(envelope);
        }

        public string Name { get { return "Backplane"; } }

        public void Start(RunningNode context)
        {
            _messageBus = context.MessageBus;
            _workerThreadId = _messageBus.StartDedicatedWorkerThread();
            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Subscribe<MessageEnvelope>(
                MessageEnvelope.SendEventName,
                e => _backplane.Send(e),
                IsMessageSendable,
                SubscribeOptions.SpecificThread(_workerThreadId)
            );
            _subscriptions.Subscribe<NodeStatus>(NodeStatus.BroadcastEvent, BroadcastNodeStatus);

            // Forward messages from the backplane to the IMessageBus
            _backplane.MessageReceived += MessageReceivedHandler;
            _backplane.ClusterMember += ClusterMemberHandler;
            _backplane.ZoneMember += ZoneMemberHandler;

            // _backplane.Start fills in Context.NodeId
            _backplane.Start(context);
            _nodeId = context.NodeId;
        }

        public void Stop()
        {
            _backplane.Stop();

            _backplane.MessageReceived -= MessageReceivedHandler;
            _backplane.ClusterMember -= ClusterMemberHandler;
            _backplane.ZoneMember -= ZoneMemberHandler;

            _subscriptions.Dispose();
            _subscriptions = null;
            _messageBus.StopDedicatedWorkerThread(_workerThreadId);
        }

        public void Dispose()
        {
            Stop();
            _messageBus.StopDedicatedWorkerThread(_workerThreadId);
        }

        private void ZoneMemberHandler(object sender, PayloadEventArgs<ZoneMemberEvent> e)
        {
            if (_messageBus != null)
                _messageBus.Publish(e);
        }

        private void ClusterMemberHandler(object sender, PayloadEventArgs<ClusterMemberEvent> e)
        {
            if (_messageBus != null)
                _messageBus.Publish(e);
        }

        private void MessageReceivedHandler(object sender, PayloadEventArgs<MessageEnvelope> e)
        {
            if (_messageBus != null)
                _messageBus.Publish(e);
        }

        private bool IsMessageSendable(MessageEnvelope envelope)
        {
            return envelope.Header.ToType == TargetType.Node ||
                   envelope.Header.ToType == TargetType.Zone ||
                   (envelope.Header.ProxyNodeId.HasValue && envelope.Header.ProxyNodeId.Value != Guid.Empty);
        }
    }
}