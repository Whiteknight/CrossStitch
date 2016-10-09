using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messaging.PubSub;
using CrossStitch.Core.Messaging.RequestResponse;
using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging
{
    public sealed class LocalMessageBus : IMessageBus
    {
        private readonly Dictionary<string, IPubSubChannel> _pubSubChannels;
        private readonly Dictionary<string, IReqResChannel> _reqResChannels;
        private readonly MessagingWorkerThreadPool _threadPool;

        public LocalMessageBus(int numThreads = 0)
        {
            _pubSubChannels = new Dictionary<string, IPubSubChannel>();
            _reqResChannels = new Dictionary<string, IReqResChannel>();
            _threadPool = new MessagingWorkerThreadPool(numThreads);
        }

        public void StartWorkers()
        {
            _threadPool.StartAll();
        }

        public void StopWorkers()
        {
            _threadPool.StopAll(false);
        }

        public int StartDedicatedWorkerThread()
        {
            return _threadPool.StartDedicatedWorker();
        }

        public void StopDedicatedWorkerThread(int id)
        {
            _threadPool.StopDedicatedWorker(id);
        }

        public void Publish<TPayload>(string name, TPayload payload)
        {
            string key = GetPubSubKey(typeof (TPayload), name);
            if (!_pubSubChannels.ContainsKey(key))
                return;
            var channel = _pubSubChannels[key] as IPubSubChannel<TPayload>;
            if (channel == null)
                return;
            channel.Publish(payload);
        }

        public IDisposable Subscribe<TPayload>(string name, Action<TPayload> subscriber, PublishOptions options = null)
        {
            return Subscribe(name, subscriber, null, options);
        }

        public IDisposable Subscribe<TPayload>(string name, Action<TPayload> subscriber, Func<TPayload, bool> filter, PublishOptions options = null)
        {
            string key = GetPubSubKey(typeof(TPayload), name);
            if (!_pubSubChannels.ContainsKey(key))
                _pubSubChannels.Add(key, new PubSubChannel<TPayload>(_threadPool));
            var channel = _pubSubChannels[key] as IPubSubChannel<TPayload>;
            if (channel == null)
                throw new Exception("Channel has incorrect type");
            return channel.Subscribe(subscriber, filter, options ?? PublishOptions.Default);
        }

        private string GetPubSubKey(Type type, string name)
        {
            return string.Format("Type={0}:Name={1}", type.Name, name ?? string.Empty);
        }

        private string GetReqResKey(Type requestType, Type responseType, string name)
        {
            return string.Format("Request={0}:Response={1}:Name={2}", requestType.Name, responseType.Name, name ?? string.Empty);
        }

        public IBrokeredResponse<TResponse> Request<TRequest, TResponse>(string name, TRequest request)
            where TRequest : IRequest<TResponse>
        {
            string key = GetReqResKey(typeof(TRequest), typeof(TResponse), name);
            if (!_reqResChannels.ContainsKey(key))
                return new BrokeredResponse<TResponse>(new List<TResponse>());
            var channel = _reqResChannels[key] as IReqResChannel<TRequest, TResponse>;
            if (channel == null)
                return new BrokeredResponse<TResponse>(new List<TResponse>());
            return channel.Request(request);
        }

        public IBrokeredResponse<object> Request(string name, Type requestType, object request)
        {
            // TODO: Cache these lookups. 
            var requestInterface = requestType.GetInterfaces().SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
            if (requestInterface == null)
                return new BrokeredResponse<object>(new List<object>());

            var responseType = requestInterface.GetGenericArguments().Single();
            string key = GetReqResKey(requestType, responseType, name);
            if (!_reqResChannels.ContainsKey(key))
                return new BrokeredResponse<object>(new List<object>());

            var channel = _reqResChannels[key];
            var channelType = typeof(IReqResChannel<,>).MakeGenericType(requestType, responseType);
            if (!channelType.IsInstanceOfType(channel))
                return new BrokeredResponse<object>(new List<object>());

            return channelType.GetMethod("Request").Invoke(channel, new[] { request }) as IBrokeredResponse<object>;
        }

        public IDisposable Subscribe<TRequest, TResponse>(string name, Func<TRequest, TResponse> subscriber, PublishOptions options = null)
            where TRequest : IRequest<TResponse>
        {
            string key = GetReqResKey(typeof(TRequest), typeof(TResponse), name);
            if (!_reqResChannels.ContainsKey(key))
                _reqResChannels.Add(key, new ReqResChannel<TRequest, TResponse>(_threadPool));
            var channel = _reqResChannels[key] as IReqResChannel<TRequest, TResponse>;
            if (channel == null)
                throw new Exception("Channel has incorrect type");
            return channel.Subscribe(subscriber, options ?? PublishOptions.Default);
        }

        public void RunEventLoop(Func<bool> shouldStop = null, int timeoutMs = 500)
        {
            if (shouldStop == null)
                shouldStop = () => false;
            var threadContext = _threadPool.GetCurrentThread();
            while (!shouldStop())
            {
                threadContext.WaitForEvent(timeoutMs);
                var action = threadContext.GetAction();
                if (action != null)
                    action.Execute(threadContext);
            }
        }

        public void EmptyActionQueue(int max)
        {
            var threadContext = _threadPool.GetCurrentThread();
            for (int i = 0; i < max; i++)
            {
                var action = threadContext.GetAction();
                if (action == null)
                    break;
                action.Execute(threadContext);
            }
        }

        public void Dispose()
        {
            foreach (var channel in _pubSubChannels.Values)
                channel.Dispose();
            _threadPool.Dispose();
        }
    }
}
