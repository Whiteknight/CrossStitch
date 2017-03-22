using System;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Data
{
    public class DataRequest<TEntity>
        where TEntity : class, IDataEntity
    {
        public DataRequestType Type { get; set; }
        public TEntity Entity { get; set; }
        public string Id { get; set; }
        public bool Force { get; set; }

        // InPlaceUpdate is a delegate which will execute on the Data thread.
        public Action<TEntity> InPlaceUpdate { get; set; }

        public bool IsValid()
        {
            switch (Type)
            {
                case DataRequestType.Get:
                    return !string.IsNullOrEmpty(Id);
                case DataRequestType.GetAll:
                    return true;
                case DataRequestType.Save:
                    return Entity != null || (InPlaceUpdate != null && !string.IsNullOrEmpty(Id));
                case DataRequestType.Delete:
                    return !string.IsNullOrEmpty(Id);
                default:
                    return false;
            }
        }

        public static DataRequest<TEntity> GetAll()
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.GetAll
            };
        }

        public static DataRequest<TEntity> Get(string id)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Get,
                Id = id
            };
        }

        public static DataRequest<TEntity> Save(TEntity entity, bool force = false)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Save,
                Entity = entity,
                Force = force
            };
        }

        public static DataRequest<TEntity> Save(string id, Action<TEntity> updateInPlace)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Save,
                Id = id,
                InPlaceUpdate = updateInPlace
            };
        }

        public static DataRequest<TEntity> Delete(string id)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Delete,
                Id = id
            };
        }
    }
}
