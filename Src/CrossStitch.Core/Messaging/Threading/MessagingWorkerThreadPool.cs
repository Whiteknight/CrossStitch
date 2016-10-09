using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CrossStitch.Core.Messaging.Threading
{
    public class MessagingWorkerThreadPool : IDisposable
    {
        private readonly List<MessageHandlerThread> _freeWorkers;
        private readonly ConcurrentDictionary<int, MessageHandlerThreadContext> _detachedContexts;
        private readonly ConcurrentDictionary<int, MessageHandlerThread> _dedicatedWorkers;
        private int _currentThread;

        public MessagingWorkerThreadPool(int numFreeWorkers)
        {
            if (numFreeWorkers < 0)
                throw new ArgumentOutOfRangeException("numFreeWorkers");
            _freeWorkers = new List<MessageHandlerThread>();
            for (int i = 0; i < numFreeWorkers; i++)
            {
                var context = new MessageHandlerThreadContext();
                var thread = new MessageHandlerThread(context);
                _freeWorkers.Add(thread);
            }
            _dedicatedWorkers = new ConcurrentDictionary<int, MessageHandlerThread>();
            _detachedContexts = new ConcurrentDictionary<int, MessageHandlerThreadContext>();
            _currentThread = 0;
        }

        public void StartAll()
        {
            foreach (var thread in _freeWorkers)
                thread.Start();
        }

        public void StopAll(bool includeDedicated)
        {
            foreach (var thread in _freeWorkers)
                thread.Stop();
            if (includeDedicated)
            {
                foreach (var thread in _dedicatedWorkers.Values)
                    thread.Stop();
            }
        }

        public int StartDedicatedWorker()
        {
            var context = new MessageHandlerThreadContext();
            var worker = new MessageHandlerThread(context);
            worker.Start();
            bool ok = _dedicatedWorkers.TryAdd(worker.ThreadId, worker);
            if (!ok)
                return 0;
            return worker.ThreadId;
        }

        public void StopDedicatedWorker(int threadId)
        {
            MessageHandlerThread worker;
            bool ok = _dedicatedWorkers.TryRemove(threadId, out worker);
            if (ok)
            {
                worker.Stop();
                worker.Dispose();
            }
        }

        public MessageHandlerThreadContext GetThread(int threadId)
        {
            MessageHandlerThread worker;
            bool ok = _dedicatedWorkers.TryGetValue(threadId, out worker);
            if (ok)
                return worker.Context;
            return _freeWorkers.Where(t => t.ThreadId == threadId).Select(t => t.Context).FirstOrDefault();
        }

        public MessageHandlerThreadContext GetAnyThread()
        {
            if (_freeWorkers.Count == 0)
                return null;
            _currentThread = (_currentThread + 1) % _freeWorkers.Count;
            return _freeWorkers[_currentThread].Context;
        }

        public MessageHandlerThreadContext GetCurrentThread()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var context = GetThread(currentThreadId);
            if (context != null)
                return context;

            context = _detachedContexts.GetOrAdd(currentThreadId, id => CreateDetachedContext());
            return context;
        }

        private MessageHandlerThreadContext CreateDetachedContext()
        {
            return new MessageHandlerThreadContext();
        }

        public void Dispose()
        {
            StopAll(true);

            foreach (var thread in _freeWorkers)
                thread.Dispose();
            _freeWorkers.Clear();

            foreach (var thread in _dedicatedWorkers.Values)
                thread.Dispose();
            _dedicatedWorkers.Clear();

            foreach (var context in _detachedContexts.Values)
                context.Dispose();
            _detachedContexts.Clear();
        }
    }
}