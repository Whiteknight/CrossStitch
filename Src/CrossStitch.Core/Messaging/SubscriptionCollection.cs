using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Messaging
{
    public sealed class SubscriptionCollection : IDisposable
    {
        private readonly IMessageBus _messageBus;
        private readonly List<IDisposable> _subscriptions;

        public SubscriptionCollection(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _subscriptions = new List<IDisposable>();
        }

        public void Subscribe<TPayload>(string name, Action<TPayload> subscriber)
        {
            var subscription = _messageBus.Subscribe<TPayload>(name, subscriber);
            _subscriptions.Add(subscription);
        }

        //public void Subscribe<TRequest, TResponse>(string name, Func<TRequest, TResponse> subscriber)
        //    where TRequest : IRequest<TResponse>
        //{
        //    var subscription = _messageBus.Subscribe<TRequest, TResponse>(name, subscriber);
        //    _subscriptions.Add(subscription);
        //}

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }
    }
}
