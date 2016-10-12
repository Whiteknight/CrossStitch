using System;
using System.IO;
using System.IO.Compression;

namespace CrossStitch.Core.Apps
{
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