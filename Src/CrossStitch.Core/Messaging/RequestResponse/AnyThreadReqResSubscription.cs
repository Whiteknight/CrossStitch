using System;
using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging.RequestResponse
{
    public class AnyThreadReqResSubscription<TRequest, TResponse> : IReqResSubscription<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Func<TRequest, TResponse> _func;
        private readonly MessagingWorkerThreadPool _threadPool;
        private readonly int _timeoutMs;

        public AnyThreadReqResSubscription(Func<TRequest, TResponse> func, MessagingWorkerThreadPool threadPool, int timeoutMs)
        {
            _func = func;
            _threadPool = threadPool;
            _timeoutMs = timeoutMs;
        }

        public TResponse Request(TRequest request)
        {
            var thread = _threadPool.GetAnyThread();
            if (thread == null)
                return default(TResponse);
            var responseWaiter = new DispatchableRequest<TRequest, TResponse>(_func, request, _timeoutMs);
            thread.DispatchAction(responseWaiter);
            bool complete = responseWaiter.WaitForResponse();
            if (!complete)
                return default(TResponse);
            return responseWaiter.Response;
        }
    }
}