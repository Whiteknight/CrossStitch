using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CrossStitch.Core.Apps
{
    public class AppFileSystem
    {
        private readonly AppsConfiguration _config;
        private readonly IVersionManager _versions;

        public AppFileSystem(AppsConfiguration config, IVersionManager versions)
        {
            _config = config;
            _versions = versions;
        }

        public string SavePackageToLibrary(string appName, string componentName, Stream contents)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                Directory.CreateDirectory(libraryDirectoryPath);

            var existingVersions = Directory.EnumerateFiles(libraryDirectoryPath, "*.zip")
                .Select(f => Path.GetFileNameWithoutExtension(f));
            var nextVersion = _versions.GetNextAvailableVersion(existingVersions);

            string libraryFilePath = Path.Combine(libraryDirectoryPath, nextVersion + ".zip");

            using (var fileStream = File.Open(libraryFilePath, FileMode.Create, FileAccess.Write))
                contents.CopyTo(fileStream);

            return nextVersion;
        }

        public bool UnzipLibraryPackageToRunningBase(string appName, string componentName, string version, string instanceId)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                return false;
            string libraryFilePath = Path.Combine(libraryDirectoryPath, version + ".zip");
            var runningDirectory = GetInstanceRunningDirectory(instanceId);
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

        private string GetInstanceRunningDirectory(string instanceId)
        {
            return Path.Combine(_config.RunningAppBasePath, instanceId.ToString());
        }

        private string GetInstanceDataDirectoryPath(string instanceId)
        {
            return Path.Combine(_config.DataBasePath, instanceId.ToString());
        }

        public bool DeleteRunningInstanceDirectory(string instanceId)
        {
            var runningDirectory = GetInstanceRunningDirectory(instanceId);
            if (!Directory.Exists(runningDirectory))
                return false;

            Directory.Delete(runningDirectory, true);
            return true;
        }

        public bool DeleteDataInstanceDirectory(string instanceId)
        {
            var directory = GetInstanceDataDirectoryPath(instanceId);
            if (!Directory.Exists(directory))
                return false;

            Directory.Delete(directory, true);
            return true;
        }

        public void GetInstanceDiskUsage(string instanceId, AppResourceUsage usage)
        {
            var runningDirectory = GetInstanceRunningDirectory(instanceId);
            usage.DiskAppUsageBytes = GetDirectorySizeOnDisk(runningDirectory);

            var dataDirectory = GetInstanceDataDirectoryPath(instanceId);
            usage.DiskDataUsageBytes = GetDirectorySizeOnDisk(dataDirectory);
        }

        private long GetDirectorySizeOnDisk(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;
            if (!Directory.Exists(path))
                return 0;
            return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Aggregate(0L, (current, file) => current + file.Length);
        }
    }
}