using CrossStitch.Core.Models;

namespace CrossStitch.Backplane.Zyre.Models
{
    public class FileTransferEnvelope
    {
        public string GroupName { get; set; }
        public string JobId { get; set; }
        public string TaskId { get; set; }
        public string FileName { get; set; }
        public int TotalNumberOfPackets { get; set; }
        public int PacketNumber { get; set; }
        public InstanceAdaptorDetails Adaptor { get; set; }
        public byte[] Contents { get; set; }
    }
}
