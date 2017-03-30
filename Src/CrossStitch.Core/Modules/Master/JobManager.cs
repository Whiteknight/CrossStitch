using Acquaintance;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Modules.Master
{
    public class JobManager
    {
        private readonly MasterDataRepository _data;
        private readonly IModuleLog _log;
        private readonly IMessageBus _messageBus;

        public JobManager(IMessageBus messageBus, MasterDataRepository data, IModuleLog log)
        {
            _messageBus = messageBus;
            _data = data;
            _log = log;
        }

        public void MarkTaskComplete(string jobId, string taskId, bool success)
        {
            var job = _data.Update<CommandJob>(jobId, j => j.MarkTaskComplete(taskId, success));
            if (job.IsComplete)
            {
                _log.LogDebug("Job Id={0} is complete: {1}", jobId, success);
                var completeEvent = new JobCompleteEvent
                {
                    JobId = job.Id,
                    Status = job.Status
                };
                string channel = completeEvent.Status == JobStatusType.Success ? JobCompleteEvent.ChannelSuccess : JobCompleteEvent.ChannelFailure;
                _messageBus.Publish(channel, completeEvent);
            }
        }
    }
}
