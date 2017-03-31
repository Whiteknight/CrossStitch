using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
    public interface IStitchEventNotifier
    {
        void StitchCreated(StitchInstance instance);
        void StitchStarted(StitchInstance instance);
        void StitchStopped(StitchInstance instance);
        void StitchDeleted(string id, StitchGroupName groupName);
    }
}