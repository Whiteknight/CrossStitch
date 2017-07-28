using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class PingCommandHandler : ICommandHandler
    {
        private readonly string _nodeId;
        private readonly MasterDataRepository _data;
        private readonly JobManager _jobManager;
        protected readonly IClusterMessageSender _sender;

        public PingCommandHandler(string nodeId, MasterDataRepository data, JobManager jobManager, IClusterMessageSender sender)
        {
            _nodeId = nodeId;
            _data = data;
            _jobManager = jobManager;
            _sender = sender;
        }

        public CommandResponse Handle(CommandRequest request)
        {
            string nodeId = request.Target;
            if (nodeId == _nodeId)
                return CommandResponse.Create(true);

            var node = _data.Get<NodeStatus>(nodeId);
            if (node == null)
                return CommandResponse.Create(false);

            var job = _jobManager.CreateJob("Command=Ping");
            var subtask = job.CreateSubtask(request.Command, request.Target, nodeId);

            // Create the message and send it over the backplane
            _sender.SendCommandRequest(node.NetworkNodeId, request, job, subtask);
            _jobManager.Save(job);
            return CommandResponse.Started(job.Id);
        }

        public bool HandleLocal(CommandRequest request)
        {
            // Return true here. MasterService will send the receipt message
            return true;
        }
    }
}