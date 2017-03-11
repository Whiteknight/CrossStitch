using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Core;
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

        Guid Start(CrossStitchCore core);
        void Start2(MessageEnvelopeBuilderFactory envelopeFactory); // TODO: Fix this
        void Stop();

        // Responsible for communication between nodes in the cluster
        void Send(MessageEnvelope message);
        //TResponse Send<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request);
        //Task<TResponse> SendAsync<TRequest, TResponse>(NodeCommunicationInformation recipient, TRequest request, CancellationToken cancellation);
    }
}