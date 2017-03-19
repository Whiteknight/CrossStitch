using CrossStitch.Core.Messages.Backplane;

namespace CrossStitch.Core.Modules.Master
{
    public interface IClusterMessageSender
    {
        void Send(ClusterMessage message);
        void SendReceipt(bool success, string networkNodeId, string jobId, string taskId);
    }
}