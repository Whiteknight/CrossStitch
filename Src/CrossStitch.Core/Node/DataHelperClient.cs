using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Data.Messages;
using System;

namespace CrossStitch.Core.Node
{
    public class DataHelperClient
    {
        protected IMessageBus Bus { get; }

        public DataHelperClient(IMessageBus messageBus)
        {
            Bus = messageBus;
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            return Bus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Get(id))
                ?.Entity;
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            var response = Bus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Delete(id));
            return response.Type == DataResponseType.Success;
        }

        public TEntity Insert<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.StoreVersion = 0;
            var request = DataRequest<TEntity>.Save(entity);
            return Bus.Request<DataRequest<TEntity>, DataResponse<TEntity>>(request)?.Entity;
        }

        public TEntity Update<TEntity>(string id, Action<TEntity> update)
            where TEntity : class, IDataEntity
        {
            var request = DataRequest<TEntity>.Save(id, update);
            return Bus.Request<DataRequest<TEntity>, DataResponse<TEntity>>(request)?.Entity;
        }
    }
}
