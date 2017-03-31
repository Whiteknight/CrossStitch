using System.Collections.Generic;
using CrossStitch.Core.Models;
using System.Linq;
using CrossStitch.Core.Utility.Extensions;

namespace CrossStitch.Core.Modules.Master
{
    public class NodeStatusBuilder
    {
        private readonly string _nodeId;
        private readonly string _nodeName;
        private readonly string _networkNodeId;
        private readonly IEnumerable<string> _zones;
        private readonly IEnumerable<string> _addedModules;
        private readonly IEnumerable<StitchInstance> _stitchInstances;

        public NodeStatusBuilder(string nodeId, string nodeName, string networkNodeId, IEnumerable<string> zones, IEnumerable<string> addedModules, IEnumerable<StitchInstance> stitchInstances)
        {
            _nodeId = nodeId;
            _nodeName = nodeName;
            _networkNodeId = networkNodeId;
            _zones = zones;
            _addedModules = addedModules;
            _stitchInstances = stitchInstances;
        }

        public NodeStatus Build()
        {
            var stitches = _stitchInstances
                .Where(si => si.State != InstanceStateType.Error && si.State != InstanceStateType.Missing)
                .ToList();

            var message = new NodeStatus
            {
                Id = _nodeId,
                Name = _nodeName,
                NetworkNodeId = _networkNodeId,
                Zones = _zones.ToList(),
                RunningModules = _addedModules.OrEmptyIfNull().ToList(),
                StitchInstances = stitches
                    .Select(si => new Messages.Stitches.InstanceInformation
                    {
                        Id = si.Id,
                        GroupName = si.GroupName.ToString(),
                        State = si.State
                    })
                    .ToList(),
            };
            return message;
        }
    }
}
