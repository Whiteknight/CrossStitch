using System;
using CrossStitch.Backplane.Zyre.Networking;

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