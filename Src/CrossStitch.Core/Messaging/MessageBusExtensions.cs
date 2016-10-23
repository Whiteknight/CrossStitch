using System;
using CrossStitch.App.Events;
using CrossStitch.Core.Messaging.RequestResponse;

namespace CrossStitch.Core.Messaging
{
    public static class PubSubMessageBusExtensions
    {
        public static void Publish<TPayload>(this IMessageBus messageBus, TPayload payload)
        {
            messageBus.Publish(string.Empty, payload);
        }

        public static void Publish<TPayload>(this IMessageBus messageBus, PayloadEventArgs<TPayload> eventArgs)
        {
            messageBus.Publish(eventArgs.Command, eventArgs.Payload);
        }

        public static IDisposable Subscribe<TPayload>(this ISubscribable messageBus, string name, Action<TPayload> subscriber, PublishOptions options = null)
        {
            return messageBus.Subscribe(name, subscriber, null, options);
        }

        public static IDisposable Subscribe<TPayload>(this ISubscribable messageBus, Action<TPayload> subscriber, PublishOptions options = null)
        {
            return messageBus.Subscribe(string.Empty, subscriber, null, options);
        }

        public static IDisposable Subscribe<TPayload>(this ISubscribable messageBus, Action<TPayload> subscriber, Func<TPayload, bool> filter, PublishOptions options = null)
        {
            return messageBus.Subscribe(string.Empty, subscriber, filter, options);
        }
    }

    public static class ReqResMessageBusExtensions
    {
        public static IBrokeredResponse<TResponse> Request<TRequest, TResponse>(this IMessageBus messageBus, TRequest request)
            where TRequest : IRequest<TResponse>
        {
            return messageBus.Request<TRequest, TResponse>(string.Empty, request);
        }

        public static IBrokeredResponse<object> Request(this IMessageBus messageBus, Type requestType, object request)
        {
            return messageBus.Request(string.Empty, requestType, request);
        }

        public static IDisposable Subscribe<TRequest, TResponse>(this ISubscribable messageBus, string name, Func<TRequest, TResponse> subscriber, PublishOptions options = null)
            where TRequest : IRequest<TResponse>
        {
            return messageBus.Subscribe(name, subscriber, null, options);
        }

        public static IDisposable Subscribe<TRequest, TResponse>(this ISubscribable messageBus, Func<TRequest, TResponse> subscriber, Func<TRequest, bool> filter, PublishOptions options = null)
            where TRequest : IRequest<TResponse>
        {
            return messageBus.Subscribe(string.Empty, subscriber, null, options);
        }

        public static IDisposable Subscribe<TRequest, TResponse>(this ISubscribable messageBus, Func<TRequest, TResponse> subscriber, PublishOptions options = null)
            where TRequest : IRequest<TResponse>
        {
            return messageBus.Subscribe(string.Empty, subscriber, options);
        }   
    }
}