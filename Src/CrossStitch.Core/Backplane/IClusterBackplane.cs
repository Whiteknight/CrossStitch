using System;
using CrossStitch.Core.Networking;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Backplane
{
    public interface IClusterBackplane : IDisposable
    {
        event EventHandler<PayloadEventArgs<ClusterCommandEvent>> ClusterCommand;
        event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        void Start(RunningNode context);
        void Stop();

        // Responsible for communication between nodes in the cluster
        void Send(MessageEnvelope message);
        //TResponse Send<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request);
        //Task<TResponse> SendAsync<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request, CancellationToken cancellation);
    }
}