using System;
using System.Collections.Generic;
using System.Linq;
using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;

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

        public bool Save<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            var response = Bus
                .Request<DataRequest<TEntity>, DataResponse<TEntity>>(DataRequest<TEntity>.Save(entity));
            return response != null;
        }

        public IEnumerable<StitchInstance> GetAllInstances()
        {
            var response = Bus
                .Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(DataRequest<StitchInstance>.GetAll());
            if (response == null)
                return Enumerable.Empty<StitchInstance>();

            return response.Entities;
        }
    }
}
