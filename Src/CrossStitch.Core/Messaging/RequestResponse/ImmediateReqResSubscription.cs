using System;

namespace CrossStitch.Core.Messaging.RequestResponse
{
    public class ImmediateReqResSubscription<TRequest, TResponse> : IReqResSubscription<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Func<TRequest, TResponse> _request;

        public ImmediateReqResSubscription(Func<TRequest, TResponse> request)
        {
            _request = request;
        }

        public TResponse Request(TRequest request)
        {
            return _request(request);
        }
    }
}