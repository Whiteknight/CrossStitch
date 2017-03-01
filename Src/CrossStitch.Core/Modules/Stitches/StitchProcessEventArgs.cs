using System;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchProcessEventArgs : EventArgs
    {
        // TODO: Do we want more than just the instance ID here? Is there any other information?
        public StitchProcessEventArgs(string instanceId, bool isRunning)
        {
            InstanceId = instanceId;
            IsRunning = isRunning;
        }

        public string InstanceId { get; private set; }
        public bool IsRunning { get; set; }
    }
}