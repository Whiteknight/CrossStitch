namespace CrossStitch.Core.Messages.Master
{
    public class CommandRequest
    {
        public CommandType Command { get; set; }
        public string Target { get; set; }
        public string ReplyToJobId { get; set; }
        public string ReplyToTaskId { get; set; }

        public bool RequestsReceipt()
        {
            return !string.IsNullOrEmpty(ReplyToJobId) && !string.IsNullOrEmpty(ReplyToTaskId);
        }

        public void SetJobDetails(string jobId, string taskId)
        {
            ReplyToJobId = jobId;
            ReplyToTaskId = taskId;
        }
    }
}
