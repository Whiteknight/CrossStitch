using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Stitch.Events;
using System;

namespace CrossStitch.Backplane.Zyre
{
    public interface IClusterBackplane : IDisposable
    {
        event EventHandler<PayloadEventArgs<MessageEnvelope>> MessageReceived;
        event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        BackplaneContext Start();
        void Stop();

        // Responsible for communication between nodes in the cluster
        void Send(MessageEnvelope envelope);
        //TResponse Send<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request);
        //Task<TResponse> SendAsync<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request, CancellationToken cancellation);
    }
}