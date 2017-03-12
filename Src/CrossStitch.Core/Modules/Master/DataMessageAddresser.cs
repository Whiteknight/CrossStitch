using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Models;
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

            // TODO: lookup all stitch instances across the cluster node statuses, and address a 
            // message to each one.

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

            // TODO: Lookup the stitch instance in the node statuses, get the node ID, address the
            // message to the appropriate node.
            return null;
        }
    }
}
