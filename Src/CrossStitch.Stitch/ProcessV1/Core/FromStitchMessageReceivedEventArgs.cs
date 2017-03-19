using System;

namespace CrossStitch.Stitch.ProcessV1.Core
{
    public class FromStitchMessageReceivedEventArgs : EventArgs
    {
        public FromStitchMessage Message { get; }

        public FromStitchMessageReceivedEventArgs(FromStitchMessage message)
        {
            Message = message;
        }
    }
}