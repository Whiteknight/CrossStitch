using System;
using System.Collections.Generic;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Utility
{
    public interface IDataRepository
    {
        bool Delete<TEntity>(string id) where TEntity : class, IDataEntity;
        TEntity Get<TEntity>(string id) where TEntity : class, IDataEntity;
        IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class, IDataEntity;
        TEntity Insert<TEntity>(TEntity entity) where TEntity : class, IDataEntity;
        bool Save<TEntity>(TEntity entity, bool force = false) where TEntity : class, IDataEntity;
        TEntity Update<TEntity>(string id, Action<TEntity> update) where TEntity : class, IDataEntity;
    }
}