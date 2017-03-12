using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Master
{
    public class DataMessageAddresser
    {
        private readonly DataHelperClient _data;

        public DataMessageAddresser(DataHelperClient data)
        {
            _data = data;
        }

        public IEnumerable<StitchDataMessage> AddressMessage(StitchDataMessage message)
        {
            if (!string.IsNullOrEmpty(message.StitchInstanceId))
            {
                message = AddressInstanceMessage(message);
                return message == null ? Enumerable.Empty<StitchDataMessage>() : new[] { message };
            }
            else if (!string.IsNullOrEmpty(message.StitchGroup))
                return AddressApplicationMessage(message);
            return Enumerable.Empty<StitchDataMessage>();
        }

        // TODO: This is going to be slow without some kind of indexing in the data module
        private IEnumerable<StitchDataMessage> AddressApplicationMessage(StitchDataMessage message)
        {
            var messages = new List<StitchDataMessage>();
            var groupName = new StitchGroupName(message.StitchGroup);
            var stitches = _data.GetAll<StitchInstance>().Where(si => groupName.Contains(si.GroupName)).ToList();
            foreach (var stitch in stitches)
            {
                messages.Add(new StitchDataMessage
                {
                    Data = message.Data,
                    ChannelName = StitchDataMessage.ChannelSendLocal,
                    StitchInstanceId = stitch.Id
                });
            }

            // TODO: We need to filter out the current node, and only look at remote nodes
            var nodes = _data.GetAll<NodeStatus>();
            foreach (var node in nodes)
            {
                var nodeStitchInstances = node.Instances.Where(i => groupName.Contains(i.GroupName));
                foreach (var stitch in nodeStitchInstances)
                {
                    messages.Add(new StitchDataMessage
                    {
                        ChannelName = StitchDataMessage.ChannelSendEnriched,
                        Data = message.Data,
                        StitchInstanceId = stitch.Id,
                        ToNodeId = Guid.Parse(node.Id),
                        ToNetworkId = node.NetworkNodeId
                    });
                }
            }

            return messages;
        }

        private StitchDataMessage AddressInstanceMessage(StitchDataMessage message)
        {
            var localStitch = _data.Get<StitchInstance>(message.StitchInstanceId);
            if (localStitch != null)
            {
                message.ChannelName = StitchDataMessage.ChannelSendLocal;
                return message;
            }

            // TODO: This is inefficient
            var nodes = _data.GetAll<NodeStatus>();
            foreach (var node in nodes)
            {
                var remoteInstance = node.Instances.FirstOrDefault(i => i.Id == message.StitchInstanceId);
                if (remoteInstance != null)
                {
                    return new StitchDataMessage
                    {
                        ChannelName = StitchDataMessage.ChannelSendEnriched,
                        Data = message.Data,
                        StitchInstanceId = message.StitchInstanceId,
                        ToNodeId = Guid.Parse(node.Id),
                        ToNetworkId = node.NetworkNodeId
                    };
                }
            }
            return null;
        }
    }
}
