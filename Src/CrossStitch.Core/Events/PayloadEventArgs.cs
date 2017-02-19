using System;

namespace CrossStitch.App.Events
{
    public class PayloadEventArgs<TPayload> : EventArgs
    {
        public PayloadEventArgs(string command, TPayload payload)
        {
            Command = command;
            Payload = payload;
        }

        public string Command { get; private set; }

        public TPayload Payload { get; private set; }
    }
}
