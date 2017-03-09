using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Data
{
    public sealed class DataModule : IModule
    {
        public const int VersionMismatch = -1;
        public const int InvalidId = -2;
        private readonly IDataStorage _storage;
        private SubscriptionCollection _subscriptions;
        private IMessageBus _messageBus;
        private int _workerThreadId;

        public DataModule(IDataStorage storage)
        {
            _storage = storage;
        }

        public string Name => "Data";

        public void Start(CrossStitchCore core)
        {
            _messageBus = core.MessageBus;
            _workerThreadId = core.MessageBus.ThreadPool.StartDedicatedWorker();
            _subscriptions = new SubscriptionCollection(core.MessageBus);
            _subscriptions.Listen<DataRequest<Application>, DataResponse<Application>>(l => l.OnDefaultChannel().Invoke(HandleRequest).OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(l => l.OnDefaultChannel().Invoke(HandleRequest).OnThread(_workerThreadId));
            _subscriptions.Listen<DataRequest<PeerNode>, DataResponse<PeerNode>>(l => l.OnDefaultChannel().Invoke(HandleRequest).OnThread(_workerThreadId));
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

        public void Dispose()
        {
            Stop();
        }

        // TODO: Get/GetAll with a Filter predicate that we can execute on the Data thread and only
        // return values which match.
        private DataResponse<TEntity> HandleRequest<TEntity>(DataRequest<TEntity> request)
            where TEntity : class, IDataEntity
        {
            switch (request.Type)
            {
                case DataRequestType.GetAll:
                    var all = _storage.GetAll<TEntity>();
                    return DataResponse<TEntity>.FoundAll(all);
                case DataRequestType.Get:
                    var entity = _storage.Get<TEntity>(request.Id);
                    return entity == null ? DataResponse<TEntity>.NotFound() : DataResponse<TEntity>.Found(entity);
                case DataRequestType.Delete:
                    bool ok = _storage.Delete<TEntity>(request.Id);
                    return !ok ? DataResponse<TEntity>.NotFound() : DataResponse<TEntity>.Ok();
                case DataRequestType.Save:
                    return HandleSaveRequest(request);
                default:
                    return DataResponse<TEntity>.BadRequest();
            }
        }

        private DataResponse<TEntity> HandleSaveRequest<TEntity>(DataRequest<TEntity> request) where TEntity : class, IDataEntity
        {
            if (!request.IsValid())
                return DataResponse<TEntity>.BadRequest();

            // If we are doing an in-place update, we need the entity to not exist, the ID to
            // be provided, and the InPlaceUpdate delegate to be set
            if (request.Entity == null && !string.IsNullOrEmpty(request.Id) && request.InPlaceUpdate != null)
            {
                request.Entity = _storage.Get<TEntity>(request.Id);
                if (request.Entity == null)
                    return DataResponse<TEntity>.NotFound();
                request.InPlaceUpdate(request.Entity);
            }

            // If we don't have an entity at this point, it's an error
            if (request.Entity == null)
                return DataResponse<TEntity>.BadRequest();

            var version = _storage.Save(request.Entity);
            if (version == VersionMismatch)
                return DataResponse<TEntity>.VersionMismatch();
            request.Entity.StoreVersion = version;
            return DataResponse<TEntity>.Saved(request.Entity);
        }
    }
}
