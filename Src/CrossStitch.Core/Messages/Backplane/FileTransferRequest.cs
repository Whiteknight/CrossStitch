using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Backplane
{
    public class FileTransferRequest
    {
        public string NetworkNodeId { get; set; }
        public StitchGroupName GroupName { get; set; }
        public string JobId { get; set; }
        public string TaskId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public InstanceAdaptorDetails Adaptor { get; set; }
    }
}
