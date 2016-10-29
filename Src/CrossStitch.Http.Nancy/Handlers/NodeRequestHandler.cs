using System;
using Acquaintance;
using CrossStitch.Core.Node.Messages;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class NodeModule : NancyModule
    {
        public NodeModule(IMessageBus messageBus) 
            : base("/node")
        {
            Get["/"] = _ => messageBus.Request<NodeStatusRequest, NodeStatus>(new NodeStatusRequest());
        }
    }

    public class ApplicationModule : NancyModule
    {
        public ApplicationModule(IMessageBus messageBus)
        {

        }
    }
}
