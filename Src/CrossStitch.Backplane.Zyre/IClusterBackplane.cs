using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Stitch.Events;
using System;
using CrossStitch.Core.Models;

namespace CrossStitch.Backplane.Zyre
{
    public interface IClusterBackplane : IDisposable
    {
        event EventHandler<PayloadEventArgs<ClusterMessage>> MessageReceived;
        event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        BackplaneContext Start();
        void Stop();

        // Responsible for communication between nodes in the cluster
        void Send(ClusterMessage envelope);
        //TResponse Send<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request);
        //Task<TResponse> SendAsync<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request, CancellationToken cancellation);

        void TransferPackageFile(StitchGroupName groupName, string toNodeId, string filePath, string fileName, string jobId, string taskId);
    }
}