using CrossStitch.Backplane.Zyre.Networking;
using System;

namespace CrossStitch.Backplane.Zyre
{
    public class BackplaneContext
    {
        public Guid NodeNetworkId { get; set; }
        public MessageEnvelopeBuilderFactory EnvelopeFactory { get; set; }
    }
}