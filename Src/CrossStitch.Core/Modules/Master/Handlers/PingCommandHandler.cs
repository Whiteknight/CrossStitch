using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class PingCommandHandler : ICommandHandler
    {
        private readonly MasterDataRepository _data;
        protected readonly IClusterMessageSender _sender;

        public PingCommandHandler(MasterDataRepository data, IClusterMessageSender sender)
        {
            _data = data;
            _sender = sender;
        }

        public CommandResponse Handle(CommandRequest request)
        {
            string nodeId = request.Target;
            var node = _data.Get<NodeStatus>(nodeId);
            if (node == null)
                return CommandResponse.Create(false);

            // TODO: If node is the local node, don't send.

            // TODO: This job stuff is getting repeated pretty often. Abstract it so we can reuse
            var job = new CommandJob();
            var subtask = job.CreateSubtask(request.Command, request.Target, nodeId);
            job = _data.Insert(job);

            // Create the message and send it over the backplane
            //request.ReplyToNodeId = ??
            request.ReplyToJobId = job.Id;
            request.ReplyToTaskId = subtask.Id;
            var message = new ClusterMessageBuilder()
                .FromNode()
                .ToNode(node.NetworkNodeId)
                .WithObjectPayload(request)
                .Build();
            _sender.Send(message);
            return CommandResponse.Started(job.Id);
        }

        public bool HandleLocal(CommandRequest request)
        {
            // Return true here. MasterService will send the receipt message
            return true;
        }
    }
}