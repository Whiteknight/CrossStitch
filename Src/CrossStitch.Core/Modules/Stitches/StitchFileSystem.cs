using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Versions;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchFileSystem
    {
        private readonly StitchesConfiguration _config;
        private readonly IVersionManager _versions;

        public StitchFileSystem(StitchesConfiguration config, IVersionManager versions)
        {
            _config = config;
            _versions = versions;
        }

        public class SavePackageResult
        {
            public string Version { get; set; }
            public string FilePath { get; set; }
        }

        // Save the new package file, creating a new version name for it
        public SavePackageResult SavePackageToLibrary(string appName, string componentName, Stream contents)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                Directory.CreateDirectory(libraryDirectoryPath);

            var existingVersions = Directory.EnumerateFiles(libraryDirectoryPath, "*.zip")
                .Select(Path.GetFileNameWithoutExtension);
            var nextVersion = _versions.GetNextAvailableVersion(existingVersions);

            string libraryFilePath = Path.Combine(libraryDirectoryPath, nextVersion + ".zip");

            using (var fileStream = File.Open(libraryFilePath, FileMode.Create, FileAccess.Write))
                contents.CopyTo(fileStream);

            return new SavePackageResult
            {
                Version = nextVersion,
                FilePath = libraryFilePath
            };
        }

        // Save the package file with an existing version, usually send from a remote node
        public SavePackageResult SavePackageToLibrary(string appName, string componentName, string versionName, Stream contents)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, appName, componentName);
            if (!Directory.Exists(libraryDirectoryPath))
                Directory.CreateDirectory(libraryDirectoryPath);

            string libraryFilePath = Path.Combine(libraryDirectoryPath, versionName + ".zip");

            using (var fileStream = File.Open(libraryFilePath, FileMode.Create, FileAccess.Write))
                contents.CopyTo(fileStream);

            return new SavePackageResult
            {
                Version = versionName,
                FilePath = libraryFilePath
            };
        }

        public class Result
        {
            public bool Success { get; set; }
            public string Path { get; set; }

            public static Result Failure()
            {
                return new Result() { Success = false };
            }
        }

        public Result UnzipLibraryPackageToRunningBase(StitchGroupName groupName, string instanceId)
        {
            string libraryDirectoryPath = Path.Combine(_config.AppLibraryBasePath, groupName.Application, groupName.Component);
            if (!Directory.Exists(libraryDirectoryPath))
                return Result.Failure();
            string libraryFilePath = Path.Combine(libraryDirectoryPath, groupName.Version + ".zip");
            var runningDirectory = GetInstanceRunningDirectory(instanceId);
            if (Directory.Exists(runningDirectory))
                Directory.Delete(runningDirectory, true);
            Directory.CreateDirectory(runningDirectory);

            using (FileStream fileStream = File.Open(libraryFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                {
                    archive.ExtractToDirectory(runningDirectory);
                }
            }
            return new Result
            {
                Success = true,
                Path = runningDirectory
            };
        }

        public string GetInstanceDataDirectoryPath(string instanceId)
        {
            return Path.Combine(_config.DataBasePath, instanceId);
        }

        public string GetInstanceRunningDirectory(string instanceId)
        {
            return Path.Combine(_config.RunningAppBasePath, instanceId);
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

        public void GetInstanceDiskUsage(string instanceId, StitchResourceUsage usage)
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