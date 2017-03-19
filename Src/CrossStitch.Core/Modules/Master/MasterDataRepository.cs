using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterDataRepository : IDataRepository
    {
        private readonly IDataRepository _data;

        public MasterDataRepository(IDataRepository data)
        {
            _data = data;
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            return _data.Delete<TEntity>(id);
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            return _data.Get<TEntity>(id);
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity
        {
            return _data.GetAll<TEntity>();
        }

        public TEntity Insert<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            return _data.Insert<TEntity>(entity);
        }

        public bool Save<TEntity>(TEntity entity, bool force)
            where TEntity : class, IDataEntity
        {
            return _data.Save<TEntity>(entity, force);
        }

        public TEntity Update<TEntity>(string id, Action<TEntity> update)
            where TEntity : class, IDataEntity
        {
            return _data.Update<TEntity>(id, update);
        }

        public FoundStitch FindStitchInstance(string stitchId)
        {
            var instance = _data.Get<StitchInstance>(stitchId);
            if (instance != null)
            {
                return new FoundStitch
                {
                    Locale = StitchLocaleType.Local
                };
            }

            var nodes = _data.GetAll<NodeStatus>();
            var node = nodes.FirstOrDefault(n => n.Instances.Any(i => i.Id == stitchId));
            // TODO: Check that node is not the local node, in case the records are out of date
            if (node != null)
            {
                return new FoundStitch
                {
                    Locale = StitchLocaleType.Remote,
                    OwnerNodeId = node.Id
                };
            }

            return new FoundStitch
            {
                Locale = StitchLocaleType.NotFound
            };
        }
    }
}