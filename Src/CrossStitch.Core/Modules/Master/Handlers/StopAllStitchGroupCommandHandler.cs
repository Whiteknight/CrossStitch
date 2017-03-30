using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public class StopAllStitchGroupCommandHandler : StitchGroupCommandHandler
    {
        public StopAllStitchGroupCommandHandler(string nodeId, MasterDataRepository data, JobManager jobManager, IStitchRequestHandler stitches, IClusterMessageSender sender)
            : base(nodeId, data, jobManager, stitches, sender)
        {
        }

        protected override CommandType CommandType => CommandType.StopStitchInstance;

        protected override void SendLocal(CommandJob job, CommandJobTask subtask, StitchSummary stitch)
        {
            // TODO: Publish status async here and rely on the job to communicate status of the
            // request. We will need a new mechanism for this and new message types
            bool ok = _stitches.StopInstance(stitch.Id);
            subtask.Status = ok ? JobStatusType.Success : JobStatusType.Failure;
        }
    }
}