using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data.InMemory;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Data
{
    public sealed class DataModule : IModule
    {
        private readonly DataService _service;
        private readonly SubscriptionCollection _subscriptions;

        private int _workerThreadId;

        public DataModule(IMessageBus messageBus, IDataStorage storage = null)
        {
            _service = new DataService(storage ?? new InMemoryDataStorage(), new ModuleLog(messageBus, Name));
            _subscriptions = new SubscriptionCollection(messageBus);
        }

        public string Name => ModuleNames.Data;

        public void Start()
        {
            _subscriptions.Clear();
            _workerThreadId = _subscriptions.WorkerPool.StartDedicatedWorker().ThreadId;
            _subscriptions.Listen<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(l => l
                .WithDefaultTopic()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<NodeStatus>, DataResponse<NodeStatus>>(l => l
                .WithDefaultTopic()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<CommandJob>, DataResponse<CommandJob>>(l => l
                .WithDefaultTopic()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<PackageFile>, DataResponse<PackageFile>>(l => l
                .WithDefaultTopic()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
        }

        public void Stop()
        {
            _subscriptions.Dispose();
            _workerThreadId = 0;
        }

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new Dictionary<string, string>();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
