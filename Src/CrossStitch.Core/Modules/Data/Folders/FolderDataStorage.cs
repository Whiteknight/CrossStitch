using CrossStitch.Core.Models;
using CrossStitch.Stitch.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossStitch.Core.Modules.Data.Folders
{
    // TODO: The DataModule is operating on a single dedicated thread. We should have a cache here
    // so we don't have to hit disk for every get
    public class FolderDataStorage : IDataStorage
    {
        private readonly Configuration _config;

        public FolderDataStorage(Configuration config = null)
        {
            _config = config ?? Configuration.GetDefault();
            if (!Directory.Exists(_config.DataPath))
                Directory.CreateDirectory(_config.DataPath);
        }

        public bool Delete<TEntity>(string id)
            where TEntity : class, IDataEntity
        {
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, id);
            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
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

        public long Save<TEntity>(TEntity entity, bool force)
            where TEntity : class, IDataEntity
        {
            if (string.IsNullOrEmpty(entity.Id))
                return SaveNew(entity) ? entity.StoreVersion : DataService.InvalidId;

            var stored = Get<TEntity>(entity.Id);
            if (stored == null)
                return SaveNew(entity) ? entity.StoreVersion : DataService.InvalidId;

            if (force)
                entity.StoreVersion = stored.StoreVersion;
            if (entity.StoreVersion != stored.StoreVersion)
                return DataService.VersionMismatch;

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

            string contents = JsonUtility.Serialize(entity);
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
            string contents = JsonUtility.Serialize(entity);
            File.WriteAllText(filePath, contents);
        }

        private static TEntity GetEntityFromFile<TEntity>(string filePath)
        {
            string contents = File.ReadAllText(filePath);
            return JsonUtility.Deserialize<TEntity>(contents);
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
