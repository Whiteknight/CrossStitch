using System;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public interface ISendChannel : IDisposable
    {
        bool Connect(string host, int port);
        bool SendMessage(MessageEnvelope envelope);
        void Disconnect();
    }
}