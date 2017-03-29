using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Master
{
    public class JobCompleteEvent
    {
        public const string ChannelSuccess = "Success";
        public const string ChannelFailure = "Failure";

        public string JobId { get; set; }
        public JobStatusType Status { get; set; }
    }
}
