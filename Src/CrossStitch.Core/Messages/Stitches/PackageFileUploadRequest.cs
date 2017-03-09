using System.IO;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class PackageFileUploadRequest
    {
        public const string ChannelUpload = "Upload";

        public string ApplicationId { get; set; }
        public Application Application { get; set; }
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
