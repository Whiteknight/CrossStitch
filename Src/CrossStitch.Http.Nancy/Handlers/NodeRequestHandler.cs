using Acquaintance;
using CrossStitch.Core.Node.Messages;
using Nancy;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class NodeModule : NancyModule
    {
        public NodeModule(IMessageBus messageBus)
            : base("/node")
        {
            Get["/"] = _ => messageBus.Request<NodeStatusRequest, NodeStatus>(new NodeStatusRequest()).Responses.First();
        }
    }
}
