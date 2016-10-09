using System;
using System.IO;
using System.IO.Compression;
using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Apps
{
    public class AppsConfiguration
    {
        public static AppsConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<AppsConfiguration>("apps.json");
        }

        public void SetDefaults()
        {
            
        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }
    }

    public class AppsModule : IModule
    {
        private readonly AppsConfiguration _configuration;

        public AppsModule(AppsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Name { get { return "Apps"; } }
        public void Start(RunningNode context)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class AppManager
    {
        private readonly AppsConfiguration _config;
        private readonly AppFileSystem _fileSystem;

        public AppManager(AppsConfiguration config)
        {
            _config = config;
            _fileSystem = new AppFileSystem(_config);
        }
    }

    public class AppFileSystem
    {
        private readonly AppsConfiguration _config;

        public AppFileSystem(AppsConfiguration config)
        {
            _config = config;
        }

        public bool SavePackageToLibrary(string appName, string componentName, string version, Stream contents)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                Directory.CreateDirectory(libraryDirectoryPath);
            string libraryFilePath = Path.Combine(libraryDirectoryPath, version + ".zip");

            using (var fileStream = File.Open(libraryFilePath, FileMode.Create, FileAccess.Write))
                contents.CopyTo(fileStream);

            return true;
        }

        public bool UnzipLibraryToRunningBase(string appName, string componentName, string version, Guid instanceId)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                return false;
            string libraryFilePath = Path.Combine(libraryDirectoryPath, version + ".zip");
            string runningDirectory = Path.Combine(_config.RunningAppBasePath, instanceId.ToString());
            if (Directory.Exists(runningDirectory))
                return false;
            Directory.CreateDirectory(runningDirectory);

            using (var fileStream = File.Open(libraryFilePath, FileMode.Open, FileAccess.Read))
            {
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                {
                    archive.ExtractToDirectory(runningDirectory);
                }
            }
            return true;
        }

        public bool DeleteRunningInstanceDirectory(Guid instanceId)
        {
            string runningDirectory = Path.Combine(_config.RunningAppBasePath, instanceId.ToString());
            if (!Directory.Exists(runningDirectory))
                return false;

            Directory.Delete(runningDirectory, true);
            return true;
        }
    }
}
