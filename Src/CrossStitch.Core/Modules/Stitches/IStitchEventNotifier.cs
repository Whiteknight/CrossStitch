using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public interface IStitchEventNotifier
    {
        void StitchStarted(StitchInstance instance);
        void StitchStopped(StitchInstance instance);
    }
}