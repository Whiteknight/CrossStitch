using System;
using Acquaintance;
using CrossStitch.Core.Events;
using CrossStitch.Core.Modules.Backplane.Events;
using CrossStitch.Core.Node;
using CrossStitch.Core.Node.Messages;
using CrossStitch.Core.Utility.Networking;

namespace CrossStitch.Core.Modules.Backplane
{
    public sealed class BackplaneModule : IModule
    {
        private readonly IClusterBackplane _backplane;
        private IMessageBus _messageBus;
        private int _workerThreadId;
        private SubscriptionCollection _subscriptions;
        private Guid _nodeId;

        public BackplaneModule(IClusterBackplane backplane)
        {
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

        public string Name => "Backplane";

        public void Start(RunningNode context)
        {
            _messageBus = context.MessageBus;
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
            _messageBus.ThreadPool.StopDedicatedWorker(_workerThreadId);
        }

        public void Dispose()
        {
            Stop();
            _messageBus.ThreadPool.StopDedicatedWorker(_workerThreadId);
        }

        private void ZoneMemberHandler(object sender, PayloadEventArgs<ZoneMemberEvent> e)
        {
            _messageBus?.Publish(e);
        }

        private void ClusterMemberHandler(object sender, PayloadEventArgs<ClusterMemberEvent> e)
        {
            _messageBus?.Publish(e);
        }

        private void MessageReceivedHandler(object sender, PayloadEventArgs<MessageEnvelope> e)
        {
            _messageBus?.Publish(e);
        }

        private static bool IsMessageSendable(MessageEnvelope envelope)
        {
            return envelope.Header.ToType == TargetType.Node ||
                   envelope.Header.ToType == TargetType.Zone ||
                   (envelope.Header.ProxyNodeId.HasValue && envelope.Header.ProxyNodeId.Value != Guid.Empty);
        }
    }
}