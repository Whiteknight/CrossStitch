using System;
using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging.RequestResponse
{
    public class SpecificThreadReqResSubscription<TRequest, TResponse> : IReqResSubscription<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Func<TRequest, TResponse> _func;
        private readonly int _threadId;
        private readonly MessagingWorkerThreadPool _threadPool;
        private readonly int _timeoutMs;

        public SpecificThreadReqResSubscription(Func<TRequest, TResponse> func, int threadId, MessagingWorkerThreadPool threadPool, int timeoutMs)
        {
            _func = func;
            _threadId = threadId;
            _threadPool = threadPool;
            _timeoutMs = timeoutMs;
        }

        public TResponse Request(TRequest request)
        {
            var thread = _threadPool.GetThread(_threadId);
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