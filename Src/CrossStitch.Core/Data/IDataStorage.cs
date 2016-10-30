using System.Collections.Generic;

namespace CrossStitch.Core.Data
{
    public interface IDataStorage
    {
        TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity;

        IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity;

        long Save<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity;

        bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity;
    }
}
