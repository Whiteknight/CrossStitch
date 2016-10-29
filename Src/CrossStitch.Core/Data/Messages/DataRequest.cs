using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Data.Messages
{
    public enum DataRequestType
    {
        Get,
        GetAll,
        Save,
        Delete
    }

    public class DataRequest<TEntity>
        where TEntity : class, IDataEntity
    {
        public DataRequestType Type { get; set; }
        public TEntity Entity { get; set; }
        public Guid Id { get; set; }

        public static DataRequest<TEntity> GetAll()
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.GetAll
            };
        }

        public static DataRequest<TEntity> Get(Guid id)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Get,
                Id = id
            };
        }

        public static DataRequest<TEntity> Save(TEntity entity)
        {
            return new DataRequest<TEntity>
            {
                Type = DataRequestType.Save,
                Entity = entity
            };
        }
    }

    public enum DataResponseType
    {
        Success,
        GeneralFailure,
        NotFound,
        VersionMismatch
    }

    public class DataResponse<TEntity>
        where TEntity : class, IDataEntity
    {
        public DataResponseType Type { get; set; }
        public TEntity Entity { get; set; }
        public List<TEntity> Entities { get; set; }

        public static DataResponse<TEntity> BadRequest()
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.GeneralFailure
            };
        }

        public static DataResponse<TEntity> NotFound()
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.NotFound
            };
        }

        public static DataResponse<TEntity> Ok()
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.Success
            };
        }

        public static DataResponse<TEntity> Found(TEntity entity)
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.Success,
                Entity = entity
            };
        }

        public static DataResponse<TEntity> Saved(TEntity entity)
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.Success,
                Entity = entity
            };
        }

        public static DataResponse<TEntity> VersionMismatch()
        {
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.VersionMismatch
            };
        }
    }

}
