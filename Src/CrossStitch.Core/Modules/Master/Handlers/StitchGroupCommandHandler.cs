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
        protected readonly IClusterMessageSender _sender;
        protected readonly string _nodeId;

        protected StitchGroupCommandHandler(string nodeId, MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender sender)
        {
            _stitches = stitches;
            _data = data;
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

            var job = new CommandJob();

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
            job = _data.Insert(job);
            return CommandResponse.Started(job.Id);
        }

        protected abstract CommandType CommandType { get; }
        protected abstract void SendLocal(CommandJob job, CommandJobTask subtask, StitchSummary stitch);
        protected virtual CommandRequest CreateRemoteRequest(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            var request = new CommandRequest();
            request.Command = CommandType;
            request.Target = stitch.Id;
            request.ReplyToJobId = job.Id;
            request.ReplyToTaskId = subtask.Id;
            return request;
        }

        private void SendRemote(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            var request = CreateRemoteRequest(job, subtask, stitch);
            var message = new ClusterMessageBuilder()
               .FromNode()
               .ToNode(stitch.NetworkNodeId)
               .WithObjectPayload(request)
               .Build();
            _sender.Send(message);
            subtask.Status = JobStatusType.Started;
        }
    }

    public class StopAllStitchGroupCommandHandler : StitchGroupCommandHandler
    {
        public StopAllStitchGroupCommandHandler(string nodeId, MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender sender) : base(nodeId, data, stitches, sender)
        {
        }

        protected override CommandType CommandType => CommandType.StopStitchInstance;

        protected override void SendLocal(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            // TODO: Publish status async here and rely on the job to communicate status of the
            // request. We will need a new mechanism for this and new message types
            bool ok = _stitches.StopInstance(stitch.Id);
            subtask.Status = ok ? JobStatusType.Complete : JobStatusType.Failure;
        }
    }

    public class StartAllStitchGroupCommandHandler : StitchGroupCommandHandler
    {
        public StartAllStitchGroupCommandHandler(string nodeId, MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender sender) : base(nodeId, data, stitches, sender)
        {
        }

        protected override CommandType CommandType => CommandType.StartStitchInstance;

        protected override void SendLocal(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            // TODO: Publish status async here and rely on the job to communicate status of the
            // request. We will need a new mechanism for this and new message types
            bool ok = _stitches.StartInstance(stitch.Id);
            subtask.Status = ok ? JobStatusType.Complete : JobStatusType.Failure;
        }
    }
}