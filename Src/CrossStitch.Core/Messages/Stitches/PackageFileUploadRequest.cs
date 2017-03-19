using CrossStitch.Core.Models;
using System.IO;

namespace CrossStitch.Core.Messages.Stitches
{
    public class PackageFileUploadRequest
    {
        public const string ChannelUpload = "Upload";

        public StitchGroupName GroupName { get; set; }
        public string FileName { get; set; }
        public Stream Contents { get; set; }

        public bool IsValid()
        {
            return GroupName != null
                && GroupName.IsComponentGroup()
                && Contents != null
                && !string.IsNullOrEmpty(FileName)
                && Path.GetExtension(FileName).ToLowerInvariant() == ".zip";
        }
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
