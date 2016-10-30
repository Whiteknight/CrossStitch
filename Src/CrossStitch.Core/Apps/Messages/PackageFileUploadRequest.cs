using System;
using System.IO;

namespace CrossStitch.Core.Apps.Messages
{
    public class PackageFileUploadRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid ComponentId { get; set; }
        public Stream Contents { get; set; }
    }

    public class PackageFileUploadResponse
    {
        public Guid VersionId { get; set; }
        public string Version { get; set; }
    }
}
