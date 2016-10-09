using System;
using CrossStitch.Core.Messaging.RequestResponse;

namespace CrossStitch.Core.Messaging
{
    public interface IMessageBus : IDisposable
    {
        void StartWorkers();
        void StopWorkers();
        int StartDedicatedWorkerThread();
        void StopDedicatedWorkerThread(int id);
        // Used for node-internal messaging
        void Publish<TPayload>(string name, TPayload payload);
        IDisposable Subscribe<TPayload>(string name, Action<TPayload> subscriber, PublishOptions options = null);
        IDisposable Subscribe<TPayload>(string name, Action<TPayload> subscriber, Func<TPayload, bool> filter, PublishOptions options = null);

        IBrokeredResponse<TResponse> Request<TRequest, TResponse>(string name, TRequest request)
            where TRequest : IRequest<TResponse>;
        IBrokeredResponse<object> Request(string name, Type requestType, object request);

        IDisposable Subscribe<TRequest, TResponse>(string name, Func<TRequest, TResponse> subscriber, PublishOptions options = null)
            where TRequest : IRequest<TResponse>;

        void RunEventLoop(Func<bool> shouldStop = null, int timeoutMs = 500);
        void EmptyActionQueue(int max);
    }
}