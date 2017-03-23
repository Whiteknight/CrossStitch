using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using CrossStitch.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterDataRepository : IDataRepository
    {
        private readonly IDataRepository _data;
        public MasterStitchCache StitchCache { get; }

        public MasterDataRepository(string nodeId, IDataRepository data)
        {
            _data = data;
            StitchCache = new MasterStitchCache(nodeId, GetLocalStitchSummaries(), GetRemoteStitchSummaries());
        }

        // TODO: Method to re-sync the stitch cache with the data module contents?

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

        public List<StitchSummary> GetStitchesInGroup(StitchGroupName group)
        {
            // TODO: Is there ever a case where we fall back to the data module?
            return StitchCache.GetStitchSummaries().Where(si => group.Contains(si.GroupName)).ToList();
        }

        //public List<StitchSummary> GetStitchesInGroupFromData(StitchGroupName group)
        //{
        //    var seenLocal = new HashSet<string>();
        //    var summaries = new List<StitchSummary>();
        //    var instances = _data.GetAll<StitchInstance>().Where(si => group.Contains(si.GroupName)).ToList();
        //    foreach (var instance in instances)
        //    {
        //        summaries.Add(new StitchSummary
        //        {
        //            Id = instance.Id,
        //            NodeId = null,  // TODO
        //            NetworkNodeId = null,   // TODO
        //            GroupName = instance.GroupName
        //        });
        //        seenLocal.Add(instance.Id);
        //    }

        //    var nodes = _data.GetAll<NodeStatus>();
        //    foreach (var node in nodes)
        //    {
        //        foreach (var instance in node.Instances)
        //        {
        //            var groupName = new StitchGroupName(instance.GroupName);
        //            if (seenLocal.Contains(instance.Id) || !group.Contains(groupName))
        //                continue;
        //            summaries.Add(new StitchSummary
        //            {
        //                Id = instance.Id,
        //                NodeId = node.Id,
        //                NetworkNodeId = node.NetworkNodeId,
        //                GroupName = groupName
        //            });
        //        }
        //    }
        //    return summaries;
        //}

        public List<StitchSummary> GetAllStitchSummaries()
        {
            return StitchCache.GetStitchSummaries();
        }

        private List<StitchSummary> GetLocalStitchSummaries()
        {
            var summaries = new List<StitchSummary>();
            var instances = _data.GetAll<StitchInstance>().ToList();
            foreach (var instance in instances)
            {
                summaries.Add(new StitchSummary
                {
                    Id = instance.Id,
                    NodeId = null, // TODO
                    NetworkNodeId = null, // TODO
                    GroupName = instance.GroupName,
                    Locale = StitchLocaleType.Local
                });
            }
            return summaries;
        }

        private Dictionary<string, List<StitchSummary>> GetRemoteStitchSummaries()
        {
            var allSummaries = new Dictionary<string, List<StitchSummary>>();
            var nodes = _data.GetAll<NodeStatus>();
            foreach (var node in nodes)
            {
                var summaries = new List<StitchSummary>();
                foreach (var instance in node.StitchInstances)
                {
                    summaries.Add(new StitchSummary
                    {
                        Id = instance.Id,
                        NodeId = node.Id,
                        NetworkNodeId = node.NetworkNodeId,
                        GroupName = new StitchGroupName(instance.GroupName),
                        Locale = StitchLocaleType.Remote
                    });
                }
                allSummaries.Add(node.Id, summaries);
            }
            return allSummaries;
        }
    }
}