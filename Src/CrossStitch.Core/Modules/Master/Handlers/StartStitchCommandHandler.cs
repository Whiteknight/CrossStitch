using CrossStitch.Core.Messages.Master;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class StartStitchCommandHandler : StitchCommandHandler
    {
        public StartStitchCommandHandler(MasterDataRepository data, JobManager jobManager, IStitchRequestHandler stitches, IClusterMessageSender sender)
            : base(data, jobManager, stitches, sender)
        {
        }

        public override bool HandleLocal(CommandRequest request)
        {
            return _stitches.StartInstance(request.Target);
        }
    }
}