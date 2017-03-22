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
            if (!string.IsNullOrEmpty(message.ToStitchInstanceId) && message.ToStitchInstanceId != message.FromStitchInstanceId)
            {
                message = AddressInstanceMessage(message);
                return message == null ? Enumerable.Empty<StitchDataMessage>() : new[] { message };
            }
            if (!string.IsNullOrEmpty(message.ToStitchGroup))
                return AddressApplicationMessage(message);
            return Enumerable.Empty<StitchDataMessage>();
        }

        // TODO: This is going to be slow without some kind of indexing in the data module
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
            var stitch = _stitches.FirstOrDefault(si => si.Id == message.ToStitchInstanceId);
            if (stitch == null)
                return null;
            message.ToNodeId = stitch.NodeId;
            message.ToNetworkId = stitch.NetworkNodeId;
            return message;
        }
    }
}
