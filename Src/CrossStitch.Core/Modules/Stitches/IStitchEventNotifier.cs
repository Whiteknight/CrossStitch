using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
    public interface IStitchEventNotifier
    {
        void StitchStarted(StitchInstance instance);
        void StitchStopped(StitchInstance instance);
    }
}