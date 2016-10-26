using System;
using Acquaintance;
using CrossStitch.Core.Node.Messages;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class NodeRequestHandler : NancyModule
    {
        public NodeRequestHandler(IMessageBus messageBus) 
            : base("/node")
        {
            Get["/"] = _ => messageBus.Request<NodeStatusRequest, NodeStatus>(new NodeStatusRequest());
        }
    }
}
