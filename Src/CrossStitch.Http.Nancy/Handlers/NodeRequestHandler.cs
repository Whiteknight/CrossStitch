using Acquaintance;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class NodeNancyModule : NancyModule
    {
        public NodeNancyModule(IMessageBus messageBus)
            : base("/node")
        {
            Get["/"] = _ => messageBus.Request<NodeStatusRequest, NodeStatus>(new NodeStatusRequest());

            Get["/{NodeId}"] = _ =>
            {
                return messageBus.Request<NodeStatusRequest, NodeStatus>(new NodeStatusRequest
                {
                    NodeId = _.NodeId.ToString()
                });
            };
        }
    }
}
