using System;

namespace CrossStitch.Core.Messaging.PubSub
{
    public interface IPubSubChannel<TPayload> : IPubSubChannel
    {
        void Publish(TPayload payload);
        SubscriptionToken Subscribe(Action<TPayload> act, Func<TPayload, bool> filter, PublishOptions options);
    }

    public interface IPubSubChannel : IChannel
    {
    }
}