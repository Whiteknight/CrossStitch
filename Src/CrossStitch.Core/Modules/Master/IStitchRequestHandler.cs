using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;

namespace CrossStitch.Core.Modules.Master
{
    public interface IStitchRequestHandler
    {
        bool StartInstance(string instanceId);
        bool StopInstance(string instanceId);
        bool RemoveInstance(string instanceId);

        void SendStitchData(StitchDataMessage message, bool remote);
        LocalCreateInstanceResponse CreateInstances(CreateInstanceRequest request, string networkNodeId, bool remote);
    }
}