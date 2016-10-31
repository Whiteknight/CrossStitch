using System.IO;

namespace CrossStitch.Core.Apps.Messages
{
    public class PackageFileUploadRequest
    {
        public string Application { get; set; }
        public string Component { get; set; }
        public Stream Contents { get; set; }
    }

    public class PackageFileUploadResponse
    {
        public PackageFileUploadResponse(bool success, string version)
        {
            Success = success;
            Version = version;
        }

        public bool Success { get; private set; }
        public string Version { get; private set; }
    }
}
