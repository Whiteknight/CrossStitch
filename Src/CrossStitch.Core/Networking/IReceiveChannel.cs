using System;

namespace CrossStitch.Core.Networking
{
    public interface IReceiveChannel : IDisposable
    {
        int Port { get; }
        void StartListening(string host = null);
        void StartListening(string host, int port);
        void StopListening();

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}