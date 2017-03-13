using CrossStitch.Core.Modules.Data;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Messages.Data
{
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

        public static DataResponse<TEntity> FoundAll(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            return new DataResponse<TEntity>
            {
                Type = DataResponseType.Success,
                Entities = entityList
            };
        }
    }
}