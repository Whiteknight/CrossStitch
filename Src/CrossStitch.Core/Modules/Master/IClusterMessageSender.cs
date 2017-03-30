using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Master
{
    public interface IClusterMessageSender
    {
        void Send(ClusterMessage message);
        void SendReceipt(bool success, string networkNodeId, string jobId, string taskId);
        void SendPackageFile(string networkNodeId, StitchGroupName groupName, string fileName, string filePath, string jobId, string taskId);
        void SendCommandRequest(string networkNodeId, CommandRequest request, CommandJob job, CommandJobTask task);
    }
}