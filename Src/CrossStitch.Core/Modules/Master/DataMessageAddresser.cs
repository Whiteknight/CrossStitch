using CrossStitch.Core.Messages;
using CrossStitch.Core.Models;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Modules.Master
{
    public class DataMessageAddresser
    {
        private readonly string _nodeId;
        private readonly IDataRepository _data;

        public DataMessageAddresser(string nodeId, IDataRepository data)
        {
            _nodeId = nodeId;
            _data = data;
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
            var stitches = _data.GetAll<StitchInstance>()
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
                    FromStitchInstanceId = message.FromStitchInstanceId
                });
            }

            // TODO: We need to filter out the current node, and only look at remote nodes
            var nodes = _data.GetAll<NodeStatus>()
                .Where(n => n.Id != _nodeId)
                .ToList();
            foreach (var node in nodes)
            {
                var nodeStitchInstances = node.Instances
                    .Where(i => groupName.Contains(i.GroupName))
                    .Where(si => si.Id != message.FromStitchInstanceId)
                    .ToList();

                foreach (var stitch in nodeStitchInstances)
                {   
                    messages.Add(new StitchDataMessage
                    {
                        Data = message.Data,
                        ToStitchInstanceId = stitch.Id,
                        FromNetworkId = message.FromNetworkId,
                        FromNodeId = message.FromNodeId,
                        FromStitchInstanceId = message.FromStitchInstanceId,

                        // Fill in destination node details, so it gets sent over the backplane
                        ToNodeId = node.Id,
                        ToNetworkId = node.NetworkNodeId
                    });
                }
            }

            return messages;
        }

        private StitchDataMessage AddressInstanceMessage(StitchDataMessage message)
        {
            var localStitch = _data.Get<StitchInstance>(message.ToStitchInstanceId);
            if (localStitch != null)
            {
                // Clear out recipient node info, to make clear that this is a local delivery
                // Otherwise reuse the same object instance
                message.ToNodeId = null;
                message.ToNetworkId = null;
                return message;
            }

            // TODO: This is inefficient
            var nodes = _data.GetAll<NodeStatus>()
                .Where(n => n.Id != _nodeId)
                .ToList();
            foreach (var node in nodes)
            {
                var remoteInstance = node.Instances.FirstOrDefault(i => i.Id == message.ToStitchInstanceId);
                if (remoteInstance != null)
                {
                    return new StitchDataMessage
                    {
                        Data = message.Data,
                        ToStitchInstanceId = message.ToStitchInstanceId,
                        FromNetworkId = message.FromNetworkId,
                        FromNodeId = message.FromNodeId,
                        FromStitchInstanceId = message.FromStitchInstanceId,

                        // Fill in node details to send over the Backplane
                        ToNodeId = node.Id,
                        ToNetworkId = node.NetworkNodeId
                    };
                }
            }
            return null;
        }
    }
}
