using CrossStitch.Core.Models;
using System.IO;

namespace CrossStitch.Core.Messages.Stitches
{
    public class PackageFileUploadRequest
    {
        public const string ChannelLocal = "Local";
        public const string ChannelFromRemote = "FromRemote";

        public StitchGroupName GroupName { get; set; }
        public string FileName { get; set; }
        public Stream Contents { get; set; }
        public bool LocalOnly { get; set; }
        public InstanceAdaptorDetails Adaptor { get; set; }


        public bool IsValidLocalRequest()
        {
            return GroupName != null
                && GroupName.IsComponentGroup()
                && Contents != null
                && Adaptor != null
                && !string.IsNullOrEmpty(FileName)
                && Path.GetExtension(FileName).ToLowerInvariant() == ".zip";
        }

        public bool IsValidRemoteRequest()
        {
            return GroupName != null
                && GroupName.IsVersionGroup()
                && Contents != null
                && Adaptor != null
                && !string.IsNullOrEmpty(FileName)
                && Path.GetExtension(FileName).ToLowerInvariant() == ".zip";
        }
    }

    public class PackageFileUploadResponse
    {
        public PackageFileUploadResponse(bool isSuccess, StitchGroupName groupName, string filePath, string jobId = null)
        {
            IsSuccess = isSuccess;
            GroupName = groupName;
            FilePath = filePath;
        }

        public bool IsSuccess { get; }
        public StitchGroupName GroupName { get; }
        public string FilePath { get; }
        public string JobId { get; set; }
    }
}
