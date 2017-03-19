using System;
using CrossStitch.Core.Messages.Backplane;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(ClusterMessage received, ClusterMessage response)
        {
            Envelope = received;
            Response = response;
        }

        public bool HandledOk { get; set; }
        public ClusterMessage Envelope { get; private set; }
        public ClusterMessage Response { get; private set; }
    }
}