using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public abstract class StitchCommandHandler : ICommandHandler
    {
        protected readonly IStitchRequestHandler _stitches;
        protected readonly MasterDataRepository _data;
        private readonly JobManager _jobManager;
        protected readonly IClusterMessageSender _sender;

        protected StitchCommandHandler(MasterDataRepository data, JobManager jobManager, IStitchRequestHandler stitches, IClusterMessageSender sender)
        {
            _stitches = stitches;
            _data = data;
            _jobManager = jobManager;
            _sender = sender;
        }

        public abstract bool HandleLocal(CommandRequest request);

        public CommandResponse Handle(CommandRequest request)
        {
            var stitch = _data.GetStitchSummary(request.Target);
            if (stitch == null || stitch.Locale == StitchLocaleType.NotFound)
                return CommandResponse.Create(false);

            if (stitch.Locale == StitchLocaleType.Local)
            {
                // Send the request to the local Stitches module
                var ok = HandleLocal(request);
                return CommandResponse.Create(ok);
            }

            if (stitch.Locale == StitchLocaleType.Remote)
                return HandleRemote(request, stitch);

            return CommandResponse.Create(false);
        }

        private CommandResponse HandleRemote(CommandRequest request, StitchSummary stitch)
        {
            string nodeId = stitch.NodeId;
            var node = _data.Get<NodeStatus>(nodeId);
            if (node == null)
                return CommandResponse.Create(false);

            // Create a job to track status
            var job = _jobManager.CreateJob("Command=" + request.Command);
            var subtask = job.CreateSubtask(request.Command, request.Target, stitch.NodeId);

            // Create the message and send it over the backplane
            _sender.SendCommandRequest(node.NetworkNodeId, request, job, subtask);

            _jobManager.Save(job);

            return CommandResponse.Started(job.Id);
        }
    }
}