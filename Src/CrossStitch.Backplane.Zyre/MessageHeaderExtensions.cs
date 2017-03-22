using System;
using CrossStitch.Core.Messages.Backplane;

namespace CrossStitch.Backplane.Zyre
{
    public static class MessageHeaderExtensions
    {
        public static Guid GetToNetworkUuid(this MessageHeader header)
        {
            return Guid.Parse(header.ToNetworkId);
        }

        public static Guid GetFromNetworkUuid(this MessageHeader header)
        {
            return Guid.Parse(header.FromNetworkId);
        }
    }
}