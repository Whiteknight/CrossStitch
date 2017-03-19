using CrossStitch.Core.Messages.Master;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class StartStitchCommandHandler : StitchCommandHandler
    {
        public StartStitchCommandHandler(MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender sender) : base(data, stitches, sender)
        {
        }

        public override bool HandleLocal(CommandRequest request)
        {
            return _stitches.StartInstance(request.Target);
        }
    }
}