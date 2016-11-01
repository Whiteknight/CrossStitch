using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Node.Messages;
using System;
using System.Linq;

namespace CrossStitch.Core.Node
{

    public class DataHelperClient
    {
        private readonly IMessageBus _messageBus;

        public DataHelperClient(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            return _messageBus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Get(id))
                .Responses
                .Select(dr => dr.Entity)
                .First();
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            return _messageBus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Delete(id))
                .Responses
                .Select(dr => dr.Type == DataResponseType.Success)
                .FirstOrDefault();
        }

        public TEntity Insert<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.StoreVersion = 0;
            var request = DataRequest<TEntity>.Save(entity);
            var response = _messageBus.Request<DataRequest<TEntity>, DataResponse<TEntity>>(request);
            return response.Responses.Select(dr => dr.Entity).FirstOrDefault();
        }

        public TEntity Update<TEntity>(string id, Action<TEntity> update)
            where TEntity : class, IDataEntity
        {
            TEntity entity = Get<TEntity>(id);
            int maxAttempts = 5;
            while (true)
            {
                update(entity);
                var response = _messageBus
                    .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Save(entity))
                    .Responses
                    .First();
                if (response.Type == DataResponseType.Success)
                    return response.Entity;
                if (response.Type == DataResponseType.VersionMismatch)
                {
                    maxAttempts--;
                    if (maxAttempts <= 0)
                        return null;
                    // Use the entity with updated StoreVersion id and try again
                    entity = response.Entity;
                    continue;
                }
                // All other response types are errors, so we'll return null here.
                return null;
            }
        }
    }
}
