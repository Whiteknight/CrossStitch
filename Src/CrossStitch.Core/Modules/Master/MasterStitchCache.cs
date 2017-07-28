using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterStitchCache
    {
        private readonly string _nodeId;

        private Dictionary<string, List<StitchSummary>> _remoteStitches;
        private List<StitchSummary> _localStitches;
        private List<StitchSummary> _allStitches;

        public MasterStitchCache(string nodeId)
        {
            _nodeId = nodeId;
            _localStitches = new List<StitchSummary>();
            _remoteStitches = new Dictionary<string, List<StitchSummary>>();
        }

        public void Initialize(List<StitchSummary> initialLocals, Dictionary<string, List<StitchSummary>> initialRemotes)
        {
            _localStitches = initialLocals ?? new List<StitchSummary>();
            _remoteStitches = initialRemotes ?? new Dictionary<string, List<StitchSummary>>();
        }

        // These three mutator methods are thread-synchronized by the MasterModule. Only the GetStitchSummaries() method is
        // concurrent.

        public void AddNodeStatus(ReceivedEvent received, NodeStatus status)
        {
            // TODO: Should we enforce ordering? If a node status with an older version comes after one with a newer
            // version, should we reject it?
            if (status.Id == _nodeId)
                return;

            var summaries = status.StitchInstances
                .Where(ii => ii.State == InstanceStateType.Running || ii.State == InstanceStateType.Started)
                .Select(si => new StitchSummary
                {
                    Id = si.Id,
                    GroupName = new StitchGroupName(si.GroupName),
                    Locale = StitchLocaleType.Remote,
                    NetworkNodeId = received.FromNetworkId,
                    NodeId = received.FromNodeId
                })
                .ToList();
            _remoteStitches.AddOrUpdate(received.FromNodeId, summaries);
            _allStitches = null;
        }

        public void AddRemoteStitch(string nodeId, string networkNodeId, string id, StitchGroupName groupName)
        {
            var summary = new StitchSummary
            {
                GroupName = groupName,
                Id = id,
                Locale = StitchLocaleType.Remote,
                NodeId = nodeId,
                NetworkNodeId = networkNodeId
            };
            if (!_remoteStitches.ContainsKey(nodeId))
            {
                _remoteStitches.Add(nodeId, new List<StitchSummary>
                {
                    summary
                });
                return;
            }
            var summaries = _remoteStitches[nodeId]
                .Where(ss => ss.Id != id)
                .Concat(new[] { summary })
                .ToList();
            _remoteStitches[nodeId] = summaries;
            _allStitches = null;
        }

        public void RemoveRemoteStitch(string nodeId, string id)
        {
            if (!_remoteStitches.ContainsKey(nodeId))
                return;
            var summaries = _remoteStitches[nodeId]
                .Where(ss => ss.Id != id)
                .ToList();
            _remoteStitches[nodeId] = summaries;
            _allStitches = null;
        }

        public void AddLocalStitch(string id, StitchGroupName groupName)
        {
            var locals = _localStitches
                .Where(si => si.Id != id)
                .Concat(new[]
                {
                    new StitchSummary
                    {
                        Id = id,
                        GroupName = groupName,
                        Locale = StitchLocaleType.Local,
                        NodeId = _nodeId
                    }
                })
                .ToList();
            _localStitches = locals;
            _allStitches = null;
        }

        public void RemoveLocalStitch(string id)
        {
            var locals = _localStitches
                .Where(si => si.Id != id)
                .ToList();
            _localStitches = locals;
            _allStitches = null;
        }



        public List<StitchSummary> GetStitchSummaries()
        {
            var all = _allStitches;
            if (all != null)
                return all;

            var remotes = _remoteStitches.Values.ToList();
            var locals = _localStitches;

            all = remotes
                .SelectMany(s => s)
                .Concat(locals)
                .ToList();
            _allStitches = all;
            return all;
        }
    }
}