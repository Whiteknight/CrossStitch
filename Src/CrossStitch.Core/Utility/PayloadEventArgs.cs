using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossStitch.Core.Utility
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
