using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Modules.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.MessageBus
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
            var response = Bus.Request<DataRequest<TEntity>, DataResponse<TEntity>>(request);
            if (response == null || response.Type != DataResponseType.Success)
                return null;

            return response.Entity;
        }

        public TEntity Update<TEntity>(string id, Action<TEntity> update)
            where TEntity : class, IDataEntity
        {
            var request = DataRequest<TEntity>.Save(id, update);
            return Bus.Request<DataRequest<TEntity>, DataResponse<TEntity>>(request)?.Entity;
        }

        public bool Save<TEntity>(TEntity entity, bool force = false)
            where TEntity : class, IDataEntity
        {
            var response = Bus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Save(entity, force));
            return response != null;
        }

        // TODO: We need to be get all stitch instances by:
        // 1) All stitches under a given application
        // 2) All stitches under a given application.component
        // 3) All stitches under a given application.component.version
        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity
        {
            var response = Bus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.GetAll());
            if (response == null)
                return Enumerable.Empty<TEntity>();

            return response.Entities;
        }
    }
}
