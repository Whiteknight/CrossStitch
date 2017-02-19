using Acquaintance;
using CrossStitch.Core.Backplane;
using CrossStitch.Core.Backplane.Events;
using CrossStitch.Core.Master.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Master
{
    public sealed class ClusterNodeManager : IClusterNodeManager
    {
        private readonly IMessageBus _messageBus;
        private readonly ConcurrentDictionary<Guid, ClusterPeerNode> _nodes;
        private SubscriptionCollection _subscriptions;

        public ClusterNodeManager(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _nodes = new ConcurrentDictionary<Guid, ClusterPeerNode>();
        }

        public void Start()
        {
            if (_subscriptions != null)
                throw new Exception("Node manager already started");

            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Subscribe<ClusterMemberEvent>(l => l.WithChannelName(ClusterMemberEvent.EnteringEvent).Invoke(HandleNodeAdded));
            _subscriptions.Subscribe<ClusterMemberEvent>(l => l.WithChannelName(ClusterMemberEvent.ExitingEvent).Invoke(HandleNodeRemoved));
        }

        public void Stop()
        {
            if (_subscriptions == null)
                return;

            _subscriptions.Dispose();
            _subscriptions = null;
        }

        private void HandleNodeRemoved(ClusterMemberEvent obj)
        {
            ClusterPeerNode peerNode;
            bool removed = _nodes.TryRemove(obj.NodeUuid, out peerNode);
            if (!removed)
                return;
            _messageBus.Publish(NodeRemovedFromClusterEvent.EventName, new NodeRemovedFromClusterEvent
            {
                Node = peerNode
            });
        }

        private void HandleNodeAdded(ClusterMemberEvent obj)
        {
            ClusterPeerNode peerNode;
            bool exists = _nodes.TryGetValue(obj.NodeUuid, out peerNode);
            if (!exists)
            {
                peerNode = _nodes.AddOrUpdate(obj.NodeUuid,
                    uuid => new ClusterPeerNode(obj.NodeUuid, obj.NodeName, new NodeCommunicationInformation()),
                    (uuid, node) => node);
                _messageBus.Publish(NodeAddedToClusterEvent.EventName, new NodeAddedToClusterEvent
                {
                    Node = peerNode
                });
            }
        }

        public IEnumerable<ClusterPeerNode> GetNodesInCluster()
        {
            return _nodes.Values.ToList();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}