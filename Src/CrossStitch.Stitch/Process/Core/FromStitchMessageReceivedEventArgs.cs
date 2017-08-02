using System;

namespace CrossStitch.Stitch.Process.Core
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