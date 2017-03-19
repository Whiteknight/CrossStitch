namespace CrossStitch.Core.Messages.Master
{
    public class CommandReceipt
    {
        public bool Success { get; set; }
        public string ReplyToJobId { get; set; }
        public string ReplyToTaskId { get; set; }
    }
}