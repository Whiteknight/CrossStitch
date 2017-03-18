using CrossStitch.Core.Models;
using System.IO;

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
        public PackageFileUploadResponse(bool success, StitchGroupName groupName)
        {
            Success = success;
            GroupName = groupName;
        }

        public bool Success { get; }
        public StitchGroupName GroupName { get; }
    }
}
