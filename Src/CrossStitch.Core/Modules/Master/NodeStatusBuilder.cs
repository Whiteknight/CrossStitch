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
        private readonly IEnumerable<string> _addedModules;
        private readonly IEnumerable<StitchInstance> _stitchInstances;

        public NodeStatusBuilder(string nodeId, string nodeName, IEnumerable<string> addedModules, IEnumerable<StitchInstance> stitchInstances)
        {
            _nodeId = nodeId;
            _nodeName = nodeName;
            _addedModules = addedModules;
            _stitchInstances = stitchInstances;
        }

        public NodeStatus Build()
        {
            var stitches = _stitchInstances
                .Where(si => si.State == InstanceStateType.Running || si.State == InstanceStateType.Started)
                .ToList();

            var message = new NodeStatus
            {
                Id = _nodeId,
                Name = _nodeName,
                RunningModules = _addedModules.OrEmptyIfNull().ToList(),
                Instances = stitches
                    .Select(si => new Messages.Stitches.InstanceInformation
                    {
                        Id = si.Id,
                        GroupName = si.GroupName.ToString(),
                        State = si.State
                    })
                    .ToList(),

                // This gets enriched in the backplane, for now
                Zones = null
            };
            return message;
        }
    }
}
