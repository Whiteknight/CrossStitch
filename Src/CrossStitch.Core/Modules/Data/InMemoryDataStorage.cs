using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CrossStitch.Core.Modules.Data
{

    public class InMemoryDataStorage : IDataStorage
    {
        private readonly DataConfiguration _config;
        private readonly Dictionary<string, string> _allData;

        public InMemoryDataStorage(DataConfiguration config)
        {
            _config = config;
            _allData = new Dictionary<string, string>();
        }

        private string GetKey<TEntity>(string id)
        {
            return typeof(TEntity).Name + ":" + id;
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string key = GetKey<TEntity>(id);
            if (_allData.ContainsKey(key))
            {
                _allData.Remove(key);
                return true;
            }
            return false;
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string key = GetKey<TEntity>(id);
            if (!_allData.ContainsKey(key))
                return null;

            string json = _allData[key];
            return JsonConvert.DeserializeObject<TEntity>(json);
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity
        {
            string prefix = typeof(TEntity).Name + ":";
            var entities = new List<string>();
            foreach (var kvp in _allData)
            {
                if (kvp.Key.StartsWith(prefix))
                    entities.Add(kvp.Value);
            }
            var deserialized = entities.Select(s => JsonConvert.DeserializeObject<TEntity>(s));
            return deserialized;
        }

        public long Save<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            if (string.IsNullOrEmpty(entity.Id))
                return SaveNew(entity) ? entity.StoreVersion : DataModule.InvalidId;

            var stored = Get<TEntity>(entity.Id);
            if (stored == null)
                return SaveNew(entity) ? entity.StoreVersion : DataModule.InvalidId;

            if (entity.StoreVersion != stored.StoreVersion)
                return DataModule.VersionMismatch;

            SaveExisting(entity);
            return entity.StoreVersion;
        }

        private bool SaveNew<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.Id = CreateId(entity);
            entity.StoreVersion = 1;
            string key = GetKey<TEntity>(entity.Id);

            if (_allData.ContainsKey(key))
                return false;

            string json = JsonConvert.SerializeObject(entity);
            _allData.Add(key, json);
            return true;
        }

        private static string CreateId(IDataEntity dataEntity)
        {
            if (!string.IsNullOrEmpty(dataEntity.Name))
                return dataEntity.Name.Replace(' ', '_');
            return Guid.NewGuid().ToString();
        }

        private void SaveExisting<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.StoreVersion++;
            string key = GetKey<TEntity>(entity.Id);
            _allData[key] = JsonConvert.SerializeObject(entity);
        }
    }
}
