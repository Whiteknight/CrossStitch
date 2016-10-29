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

        public bool Delete<TEntity>(Guid id) 
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

        public TEntity Get<TEntity>(Guid id) 
            where TEntity : class, IDataEntity
        {
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, id);
            if (!File.Exists(filePath))
                return null;

            return GetEntityFromFile<TEntity>(filePath);
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
                Guid id;
                bool ok = Guid.TryParse(Path.GetFileNameWithoutExtension(file), out id);
                if (!ok)
                    continue;

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
            if (entity.Id != Guid.Empty)
            {
                SaveNew<TEntity>(entity);
                return entity.Version;
            }

            var stored = Get<TEntity>(entity.Id);
            if (stored == null)
            {
                SaveNew<TEntity>(entity);
                return entity.Version;
            }

            if (entity.Version != stored.Version)
                return DataModule.VersionMismatch;

            SaveExisting<TEntity>(entity);
            return entity.Version;
        }

        private void SaveNew<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.Id = Guid.NewGuid();
            entity.Version = 1;
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, entity.Id);
            string contents = JsonConvert.SerializeObject(entity);
            File.WriteAllText(filePath, contents);
        }

        private void SaveExisting<TEntity>(TEntity entity)
            where TEntity : class, IDataEntity
        {
            entity.Version++;
            string filePath = GetEntityFullFilePath<TEntity>(_config.DataPath, entity.Id);
            string contents = JsonConvert.SerializeObject(entity);
            File.WriteAllText(filePath, contents);
        }

        private TEntity GetEntityFromFile<TEntity>(string filePath)
        {
            string contents = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TEntity>(contents);
        }

        private string GetEntityDirectory<TEntity>(string basePath)
        {
            string path = Path.Combine(_config.DataPath, typeof(TEntity).Name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetEntityFileName(Guid id)
        {
            return id.ToString() + ".json";
        }

        private string GetEntityFullFilePath<TEntity>(string basePath, Guid id)
        {
            return Path.Combine(GetEntityDirectory<TEntity>(basePath), GetEntityFileName(id));
        }
    }
}
