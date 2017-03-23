using Acquaintance;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
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

            Get["/{NodeId}/stitches"] = _ =>
            {
                // TODO: Get all StitchSummaries from this node in the MasterModule
                return null;
            };
        }
    }
}
