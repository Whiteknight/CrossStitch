using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Models;
using System.Collections.Generic;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules.Data.InMemory;

namespace CrossStitch.Core.Modules.Data
{
    public sealed class DataModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly DataService _service;

        private SubscriptionCollection _subscriptions;
        private int _workerThreadId;

        public DataModule(IMessageBus messageBus, IDataStorage storage = null)
        {
            _messageBus = messageBus;
            _service = new DataService(storage ?? new InMemoryDataStorage(), new ModuleLog(messageBus, Name));
        }

        public string Name => ModuleNames.Data;

        public void Start(CrossStitchCore core)
        {
            _workerThreadId = core.MessageBus.ThreadPool.StartDedicatedWorker();

            _subscriptions = new SubscriptionCollection(core.MessageBus);
            _subscriptions.Listen<DataRequest<Application>, DataResponse<Application>>(l => l
                .OnDefaultChannel()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(l => l
                .OnDefaultChannel()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<NodeStatus>, DataResponse<NodeStatus>>(l => l
                .OnDefaultChannel()
                .Invoke(_service.HandleRequest)
                .OnThread(_workerThreadId));
        }

        public void Stop()
        {
            if (_subscriptions == null)
                return;
            _subscriptions.Dispose();
            _subscriptions = null;
            _messageBus.ThreadPool.StopDedicatedWorker(_workerThreadId);
            _workerThreadId = 0;
        }

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new Dictionary<string, string>
            {
            };
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
