using System;
using Acquaintance.RequestResponse;

namespace CrossStitch.Core.Node.Messages
{
    public class NodeStatusRequest : IRequest<NodeStatus>
    {
        public Guid NodeId { get; set; }
    }
}
