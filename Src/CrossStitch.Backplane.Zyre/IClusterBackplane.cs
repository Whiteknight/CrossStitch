using System;
using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Node;
using CrossStitch.Stitch.Events;

namespace CrossStitch.Backplane.Zyre
{
    public interface IClusterBackplane : IDisposable
    {
        event EventHandler<PayloadEventArgs<MessageEnvelope>> MessageReceived;
        event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        void Start(CrossStitchCore context);
        void Stop();

        // Responsible for communication between nodes in the cluster
        void Send(MessageEnvelope message);
        //TResponse Send<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request);
        //Task<TResponse> SendAsync<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request, CancellationToken cancellation);
    }
}