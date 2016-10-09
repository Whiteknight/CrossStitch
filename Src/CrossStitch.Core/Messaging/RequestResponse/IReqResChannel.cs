using System;

namespace CrossStitch.Core.Messaging.RequestResponse
{
    public interface IReqResChannel<TRequest, TResponse> : IReqResChannel
        where TRequest : IRequest<TResponse>
    {
        BrokeredResponse<TResponse> Request(TRequest request);
        SubscriptionToken Subscribe(Func<TRequest, TResponse> act, PublishOptions options);
    }

    public interface IReqResChannel : IChannel
    {
        
    }
}