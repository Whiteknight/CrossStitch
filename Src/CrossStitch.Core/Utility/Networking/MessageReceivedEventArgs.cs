using System;

namespace CrossStitch.App.Networking
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(MessageEnvelope received, MessageEnvelope response)
        {
            Envelope = received;
            Response = response;
        }

        public bool HandledOk { get; set; }
        public MessageEnvelope Envelope { get; private set; }
        public MessageEnvelope Response { get; private set; }
    }
}