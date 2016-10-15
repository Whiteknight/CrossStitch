using System;

namespace CrossStitch.App.Networking
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(MessageEnvelope envelope)
        {
            Envelope = envelope;
        }

        public MessageEnvelope Envelope { get; private set; }
    }
}