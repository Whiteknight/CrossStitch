using CrossStitch.Core.Models;
using System.Linq;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Modules.Master
{
    public class NodeStatusBuilder
    {
        private readonly CrossStitchCore _core;
        private readonly IDataRepository _data;

        public NodeStatusBuilder(CrossStitchCore core, IDataRepository data)
        {
            _core = core;
            _data = data;
        }

        public NodeStatus Build()
        {
            var modules = _core.Modules.AddedModules.ToList();
            var stitches = _data.GetAll<StitchInstance>()
                .Where(si => si.State == InstanceStateType.Running || si.State == InstanceStateType.Started)
                .ToList();

            var message = new NodeStatus
            {
                Id = _core.NodeId.ToString(),
                Name = _core.NodeId.ToString(),
                RunningModules = modules,
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
