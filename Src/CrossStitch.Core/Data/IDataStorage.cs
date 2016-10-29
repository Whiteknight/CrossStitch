using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Data
{
    public interface IDataStorage
    {
        TEntity Get<TEntity>(Guid id)
            where TEntity : class, IDataEntity;

        IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity;

        long Save<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity;

        bool Delete<TEntity>(Guid id)
            where TEntity : class, IDataEntity;
    }
}
