using Acquaintance;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Data
{
    public sealed class DataModule : IModule
    {
        private readonly IDataStorage _storage;
        private SubscriptionCollection _subscriptions;
        private IMessageBus _messageBus;
        private int _workerThreadId;

        public DataModule(IDataStorage storage)
        {
            _storage = storage;
        }

        public string Name => "Data";

        public void Start(RunningNode context)
        {
            _messageBus = context.MessageBus;
            _workerThreadId = context.MessageBus.StartDedicatedWorkerThread();
            var options = new SubscribeOptions
            {
                DispatchType = Acquaintance.Threading.DispatchThreadType.SpecificThread,
                ThreadId = _workerThreadId
            };
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _subscriptions.Listen<DataRequest<Application>, DataResponse<Application>>(HandleRequest, options);
            _subscriptions.Listen<DataRequest<Instance>, DataResponse<Instance>>(HandleRequest, options);
            _subscriptions.Listen<DataRequest<PeerNode>, DataResponse<PeerNode>>(HandleRequest, options);
        }

        public void Stop()
        {
            if (_subscriptions == null)
                return;
            _subscriptions.Dispose();
            _subscriptions = null;
            _messageBus.StopDedicatedWorkerThread(_workerThreadId);
            _workerThreadId = 0;
        }

        public void Dispose()
        {
            Stop();
        }

        public const int VersionMismatch = -1;

        private DataResponse<TEntity> HandleRequest<TEntity>(DataRequest<TEntity> request)
            where TEntity : class, IDataEntity
        {
            if (request.Type == DataRequestType.GetAll)
            {
                var all = _storage.GetAll<TEntity>();
                return DataResponse<TEntity>.FoundAll(all);
            }

            if (request.Type == DataRequestType.Get)
            {
                var entity = _storage.Get<TEntity>(request.Id);
                if (entity == null)
                    return DataResponse<TEntity>.NotFound();
                return DataResponse<TEntity>.Found(entity);
            }

            if (request.Type == DataRequestType.Delete)
            {
                bool ok = _storage.Delete<TEntity>(request.Id);
                if (!ok)
                    return DataResponse<TEntity>.NotFound();
                return DataResponse<TEntity>.Ok();
            }

            if (request.Type == DataRequestType.Save)
            {
                if (request.Entity == null)
                    return DataResponse<TEntity>.BadRequest();

                var version = _storage.Save(request.Entity);
                if (version == VersionMismatch)
                    return DataResponse<TEntity>.VersionMismatch();
                return DataResponse<TEntity>.Saved(request.Entity);
            }

            return DataResponse<TEntity>.BadRequest();
        }
    }
}
