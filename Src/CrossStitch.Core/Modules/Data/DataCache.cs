using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Data
{
    // TODO: This is just a prototype and needs serious testing.
    public class DataCache : IDataStorage
    {
        private readonly Dictionary<string, string> _allData;
        private readonly IDataStorage _inner;

        public DataCache(IDataStorage inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            _allData = new Dictionary<string, string>();
            _inner = inner;
        }

        private string GetKey<TEntity>(string id)
        {
            return typeof(TEntity).Name + ":" + id;
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            bool ok = _inner.Delete<TEntity>(id);
            if (!ok)
                return false;

            string key = GetKey<TEntity>(id);
            if (_allData.ContainsKey(key))
                _allData.Remove(key);
            return ok;
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string json;
            string key = GetKey<TEntity>(id);
            if (_allData.ContainsKey(key))
            {
                json = _allData[key];
                return JsonConvert.DeserializeObject<TEntity>(json);
            }

            var entity = _inner.Get<TEntity>(id);
            if (entity == null)
                return null;
            json = JsonConvert.SerializeObject(entity);
            _allData.Add(key, json);
            return entity;
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity
        {
            // No real way to cache this, because we don't know if we have all data. We could
            // iterate over the results and make sure they are in the cache in case we want to
            // get each one individually later.s
            return _inner.GetAll<TEntity>();
        }

        public long Save<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            long id = _inner.Save<TEntity>(entity);
            string key = GetKey<TEntity>(entity.Id);
            string json = JsonConvert.SerializeObject(entity);
            _allData.Add(key, json);
            return id;
        }
    }
}
