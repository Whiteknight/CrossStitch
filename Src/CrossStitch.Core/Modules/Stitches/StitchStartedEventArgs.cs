using System;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchStartedEventArgs : EventArgs
    {
        public StitchStartedEventArgs(string instanceId)
        {
            InstanceId = instanceId;
        }

        public string InstanceId { get; private set; }
    }
}