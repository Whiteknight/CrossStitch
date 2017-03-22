using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public abstract class StitchCommandHandler : ICommandHandler
    {
        protected readonly IStitchRequestHandler _stitches;
        protected readonly MasterDataRepository _data;
        protected readonly IClusterMessageSender _sender;

        protected StitchCommandHandler(MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender sender)
        {
            _stitches = stitches;
            _data = data;
            _sender = sender;
        }

        public abstract bool HandleLocal(CommandRequest request);

        public CommandResponse Handle(CommandRequest request)
        {
            var stitchLocation = _data.FindStitchInstance(request.Target);
            if (stitchLocation.Locale == StitchLocaleType.NotFound)
                return CommandResponse.Create(false);

            if (stitchLocation.Locale == StitchLocaleType.Local)
            {
                // Send the request to the local Stitches module
                var ok = HandleLocal(request);
                return CommandResponse.Create(ok);
            }

            if (stitchLocation.Locale == StitchLocaleType.Remote)
            {
                string nodeId = stitchLocation.OwnerNodeId;
                var node = _data.Get<NodeStatus>(nodeId);
                if (node == null)
                    return CommandResponse.Create(false);

                // Create a job to track status
                var job = new CommandJob();
                var subtask = job.CreateSubtask(request.Command, request.Target, stitchLocation.OwnerNodeId);
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

            return CommandResponse.Create(false);
        }
    }
}