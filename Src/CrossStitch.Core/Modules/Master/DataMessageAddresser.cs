using CrossStitch.Core.Messages;
using CrossStitch.Core.Models;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Modules.Master.Models;

namespace CrossStitch.Core.Modules.Master
{
    public class DataMessageAddresser
    {
        private readonly IReadOnlyList<StitchSummary> _stitches;

        public DataMessageAddresser(IReadOnlyList<StitchSummary> stitches)
        {
            _stitches = stitches;
        }

        public IEnumerable<StitchDataMessage> AddressMessage(StitchDataMessage message)
        {
            var fullFromId = new StitchFullId(message.FromNodeId, message.FromStitchInstanceId);
            return AddressMessageInternal(message)
                .Where(s => new StitchFullId(s.ToNodeId, s.ToStitchInstanceId).FullId != fullFromId.FullId);
        }

        private IEnumerable<StitchDataMessage> AddressMessageInternal(StitchDataMessage message)
        {
            if (!string.IsNullOrEmpty(message.ToStitchInstanceId))
            {
                message = AddressInstanceMessage(message);
                return message == null ? Enumerable.Empty<StitchDataMessage>() : new[] { message };
            }
            if (!string.IsNullOrEmpty(message.ToStitchGroup))
                return AddressApplicationMessage(message);
            return Enumerable.Empty<StitchDataMessage>();
        }

        private IEnumerable<StitchDataMessage> AddressApplicationMessage(StitchDataMessage message)
        {
            var messages = new List<StitchDataMessage>();
            var groupName = new StitchGroupName(message.ToStitchGroup);
            var stitches = _stitches
                .Where(si => groupName.Contains(si.GroupName))
                .Where(si => si.Id != message.FromStitchInstanceId)
                .ToList();

            foreach (var stitch in stitches)
            {
                messages.Add(new StitchDataMessage
                {
                    // Leave the ToNodeID and NetworkID empty, so it gets routed locally
                    Data = message.Data,
                    ToStitchInstanceId = stitch.Id,
                    FromNetworkId = message.FromNetworkId,
                    FromNodeId = message.FromNodeId,
                    FromStitchInstanceId = message.FromStitchInstanceId,
                    ToNetworkId = stitch.NetworkNodeId,
                    ToNodeId = stitch.NodeId
                });
            }

            return messages;
        }

        private StitchDataMessage AddressInstanceMessage(StitchDataMessage message)
        {
            var fullToId = new StitchFullId(message.ToNodeId, message.ToStitchInstanceId);

            IEnumerable<StitchSummary> query = _stitches;
            var toNodeId = message.ToNodeId;
            if (string.IsNullOrEmpty(toNodeId) && !fullToId.IsLocalOnly)
                toNodeId = fullToId.NodeId;
            if (!string.IsNullOrEmpty(toNodeId))
                query = query.Where(s => s.NodeId == toNodeId);

            var stitch = query.FirstOrDefault(si => si.Id == fullToId.StitchInstanceId);
            if (stitch == null)
                return null;

            message.ToNodeId = stitch.NodeId;
            message.ToNetworkId = stitch.NetworkNodeId;
            return message;
        }
    }
}
