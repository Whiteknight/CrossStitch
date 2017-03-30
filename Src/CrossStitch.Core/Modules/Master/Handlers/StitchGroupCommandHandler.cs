using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public abstract class StitchGroupCommandHandler : ICommandHandler
    {
        protected readonly IStitchRequestHandler _stitches;
        protected readonly MasterDataRepository _data;
        private readonly JobManager _jobManager;
        protected readonly IClusterMessageSender _sender;
        protected readonly string _nodeId;

        protected StitchGroupCommandHandler(string nodeId, MasterDataRepository data, JobManager jobManager, IStitchRequestHandler stitches, IClusterMessageSender sender)
        {
            _stitches = stitches;
            _data = data;
            _jobManager = jobManager;
            _sender = sender;
            _nodeId = nodeId;
        }

        public bool HandleLocal(CommandRequest request)
        {
            // We should never get here, so this does nothing. The originating Master module will
            // generate atomic commands for individual stitches and send them, never sending large
            // group-aggregate messages like this.
            return true;
        }

        public CommandResponse Handle(CommandRequest request)
        {
            if (string.IsNullOrEmpty(request.Target))
                return CommandResponse.Create(false);
            var groupName = new StitchGroupName(request.Target);

            var job = _jobManager.CreateJob("Command=" + request.Command);

            var stitches = _data.GetStitchesInGroup(groupName);
            foreach (var stitch in stitches)
            {
                var subtask = job.CreateSubtask(CommandType, stitch.Id, _nodeId);
                if (stitch.NodeId == _nodeId)
                    SendLocal(job, subtask, stitch);
                else
                    SendRemote(job, subtask, stitch);
            }

            // Save the job to get an Id
            _jobManager.Save(job);
            return CommandResponse.Started(job.Id);
        }

        protected abstract CommandType CommandType { get; }
        protected abstract void SendLocal(CommandJob job, CommandJobTask subtask, StitchSummary stitch);

        protected virtual CommandRequest CreateRemoteRequest(StitchSummary stitch)
        {
            return new CommandRequest
            {
                Command = CommandType
            };
        }

        private void SendRemote(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            var request = CreateRemoteRequest(stitch);
            request.Target = stitch.Id;
            _sender.SendCommandRequest(stitch.NetworkNodeId, request, job, subtask);
        }
    }
}