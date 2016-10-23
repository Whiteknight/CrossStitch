using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging.RequestResponse
{
    public class ReqResChannel<TRequest, TResponse> : IReqResChannel<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly MessagingWorkerThreadPool _threadPool;
        private readonly Dictionary<Guid, IReqResSubscription<TRequest, TResponse>> _subscriptions;

        public ReqResChannel(MessagingWorkerThreadPool threadPool)
        {
            _threadPool = threadPool;
            _subscriptions = new Dictionary<Guid, IReqResSubscription<TRequest, TResponse>>();
        }

        public void Unsubscribe(Guid id)
        {
            _subscriptions.Remove(id);
        }

        public BrokeredResponse<TResponse> Request(TRequest request)
        {
            List<TResponse> responses = new List<TResponse>();
            foreach (var subscription in _subscriptions.Values.Where(s => s.CanHandle(request)))
            {
                var response = subscription.Request(request);
                responses.Add(response);
            }
            return new BrokeredResponse<TResponse>(responses);
        }

        public SubscriptionToken Subscribe(Func<TRequest, TResponse> act,Func<TRequest, bool> filter, PublishOptions options)
        {
            Guid id = Guid.NewGuid();
            var subscription = CreateSubscription(act, filter, options);
            _subscriptions.Add(id, subscription);
            return new SubscriptionToken(this, id);
        }

        public void Dispose()
        {
            _subscriptions.Clear();
        }

        private IReqResSubscription<TRequest, TResponse> CreateSubscription(Func<TRequest, TResponse> func, Func<TRequest, bool> filter, PublishOptions options)
        {
            switch (options.DispatchType)
            {
                case DispatchThreadType.AnyWorkerThread:
                    return new AnyThreadReqResSubscription<TRequest, TResponse>(func, filter, _threadPool, options.WaitTimeoutMs);
                case DispatchThreadType.SpecificThread:
                    return new SpecificThreadReqResSubscription<TRequest, TResponse>(func, filter, options.ThreadId, _threadPool, options.WaitTimeoutMs);
                default:
                    return new ImmediateReqResSubscription<TRequest, TResponse>(func, filter);
            }
        }
    }
}