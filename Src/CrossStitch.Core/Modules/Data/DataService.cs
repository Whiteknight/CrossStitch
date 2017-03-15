using System;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Data;

namespace CrossStitch.Core.Modules.Data
{
    // TODO: We need indexes/caches for the following scenarios:
    // 1) Given a stitch ID, get the NodeId where it lives
    // 2) Given a group name, get all stitchId+nodeId pairs in that group.
    public class DataService
    {
        public const int VersionMismatch = -1;
        public const int InvalidId = -2;

        private readonly IDataStorage _storage;
        private readonly ModuleLog _log;

        public DataService(IDataStorage storage, ModuleLog log)
        {
            _storage = storage;
            _log = log;
        }

        // TODO: Get/GetAll with a Filter predicate that we can execute on the Data thread and only
        // return values which match.
        public DataResponse<TEntity> HandleRequest<TEntity>(DataRequest<TEntity> request)
            where TEntity : class, IDataEntity
        {
            try
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
            catch (Exception e)
            {
                _log.LogError(e, "Error handling data request");
                return DataResponse<TEntity>.BadRequest();
            }
        }

        private DataResponse<TEntity> HandleSaveRequest<TEntity>(DataRequest<TEntity> request)
            where TEntity : class, IDataEntity
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

            var version = _storage.Save(request.Entity, request.Force);
            if (version == VersionMismatch)
                return DataResponse<TEntity>.VersionMismatch();
            request.Entity.StoreVersion = version;
            return DataResponse<TEntity>.Saved(request.Entity);
        }
    }
}