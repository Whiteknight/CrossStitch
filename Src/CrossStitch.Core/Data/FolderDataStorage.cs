using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossStitch.Core.Data
{
    public class FolderDataStorage : IDataStorage
    {
        private readonly DataConfiguration _config;

        public FolderDataStorage(DataConfiguration config)
        {
            _config = config;
            if (!Directory.Exists(config.DataPath))
                Directory.CreateDirectory(config.DataPath);
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public TEntity Get<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, id);
            return !File.Exists(filePath) ? null : GetEntityFromFile<TEntity>(filePath);
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
            where TEntity : class, IDataEntity
        {
            string entityDirectoryPath = GetEntityDirectory<TEntity>(_config.DataPath);
            if (!Directory.Exists(entityDirectoryPath))
                return Enumerable.Empty<TEntity>();

            List<TEntity> entities = new List<TEntity>();
            foreach (string file in Directory.EnumerateFiles(entityDirectoryPath))
            {
                var entity = GetEntityFromFile<TEntity>(file);
                if (entity == null)
                    continue;
                entities.Add(entity);
            }
            return entities;
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
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, entity.Id);
            if (File.Exists(filePath))
                return false;

            string contents = JsonConvert.SerializeObject(entity);
            File.WriteAllText(filePath, contents);
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
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, entity.Id);
            string contents = JsonConvert.SerializeObject(entity);
            File.WriteAllText(filePath, contents);
        }

        private static TEntity GetEntityFromFile<TEntity>(string filePath)
        {
            string contents = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TEntity>(contents);
        }

        private string GetEntityDirectory<TEntity>(string basePath)
        {
            string path = Path.Combine(basePath, typeof(TEntity).Name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static string GetEntityFileName(string id)
        {
            return id + ".json";
        }

        private string GetEntityFullFilePath<TEntity>(string basePath, string id)
        {
            return Path.Combine(GetEntityDirectory<TEntity>(basePath), GetEntityFileName(id));
        }
    }
}
