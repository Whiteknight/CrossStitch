using System.Collections.Generic;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Data
{
    public interface IDataStorage
    {
        TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity;

        IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity;

        long Save<TEntity>(TEntity entity, bool force)
            where TEntity : class, IDataEntity;

        bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity;
    }
}
