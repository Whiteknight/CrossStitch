using Acquaintance;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
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
}
