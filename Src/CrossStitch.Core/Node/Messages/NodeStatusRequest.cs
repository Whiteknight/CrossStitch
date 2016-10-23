using System;
using CrossStitch.Core.Messaging.RequestResponse;

namespace CrossStitch.Core.Node.Messages
{
    public class NodeStatusRequest : IRequest<NodeStatus>
    {
        public Guid NodeId { get; set; }
    }
}
