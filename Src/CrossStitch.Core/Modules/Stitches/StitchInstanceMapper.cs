using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchInstanceMapper
    {
        private readonly string _nodeId;
        private readonly string _nodeName;

        public StitchInstanceMapper(string nodeId, string nodeName)
        {
            _nodeId = nodeId;
            _nodeName = nodeName;
        }

        public StitchInstance Map(LocalCreateInstanceRequest request)
        {
            return new StitchInstance
            {
                Id = null,
                StoreVersion = 0,
                Adaptor = request.Adaptor,
                GroupName = request.GroupName,
                LastHeartbeatReceived = 0,
                Name = request.Name,
                OwnerNodeName = _nodeName,
                OwnerNodeId = _nodeId
            };
        }
    }
}
