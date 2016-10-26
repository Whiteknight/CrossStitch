using System;
using CrossStitch.App.Networking;
using Acquaintance;
using CrossStitch.Core.Node;
using CrossStitch.Core.Node.Messages;

namespace CrossStitch.Core.Backplane
{
    public sealed class BackplaneModule : IModule
    {
        private readonly BackplaneConfiguration _configuration;
        private readonly IClusterBackplane _backplane;
        private readonly IMessageBus _messageBus;
        private readonly int _workerThreadId;
        private readonly SubscriptionCollection _subscriptions;
        private Guid _nodeId;

        public BackplaneModule(BackplaneConfiguration configuration, IClusterBackplane backplane, IMessageBus messageBus)
        {
            _configuration = configuration;
            _backplane = backplane;
            _messageBus = messageBus;
            _subscriptions = new SubscriptionCollection(messageBus);

            // Forward messages from the backplane to the IMessageBus
            _backplane.MessageReceived += (s, e) => _messageBus.Publish(e);
            _backplane.ClusterMember += (s, e) => _messageBus.Publish(e);
            _backplane.ZoneMember += (s, e) => _messageBus.Publish(e);

            // Forward messages from the IMessageBus to the backplane
            _workerThreadId = _messageBus.StartDedicatedWorkerThread();
            _subscriptions.Subscribe<MessageEnvelope>(
                MessageEnvelope.SendEventName, 
                e => _backplane.Send(e), 
                IsMessageSendable,
                SubscribeOptions.SpecificThread(_workerThreadId)
            );
            _subscriptions.Subscribe<NodeStatus>(NodeStatus.BroadcastEvent, BroadcastNodeStatus);
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
            // _backplane.Start fills in Context.NodeId
            _backplane.Start(context);
            _nodeId = context.NodeId;
        }

        public void Stop()
        {
            _backplane.Stop();
        }

        public void Dispose()
        {
            Stop();
            _messageBus.StopDedicatedWorkerThread(_workerThreadId);
        }

        private bool IsMessageSendable(MessageEnvelope envelope)
        {
            return envelope.Header.ToType == TargetType.Node ||
                   envelope.Header.ToType == TargetType.Zone ||
                   (envelope.Header.ProxyNodeId.HasValue && envelope.Header.ProxyNodeId.Value != Guid.Empty);
        }
    }
}