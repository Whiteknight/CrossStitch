using CrossStitch.Core.Messages.Master;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class StopStitchCommandHandler : StitchCommandHandler
    {
        public StopStitchCommandHandler(MasterDataRepository data, JobManager jobManager, IStitchRequestHandler stitches, IClusterMessageSender sender)
            : base(data, jobManager, stitches, sender)
        {
        }

        public override bool HandleLocal(CommandRequest request)
        {
            return _stitches.StopInstance(request.Target);
        }
    }
}