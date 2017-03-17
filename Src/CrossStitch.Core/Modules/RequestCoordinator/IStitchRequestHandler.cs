using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public interface IStitchRequestHandler
    {
        StitchInstance StartInstance(StitchInstance instance);
        StitchInstance StopInstance(StitchInstance instance);
        StitchInstance CreateInstance(StitchInstance instance);
        PackageFileUploadResponse UploadStitchPackageFile(PackageFileUploadRequest request);
    }
}