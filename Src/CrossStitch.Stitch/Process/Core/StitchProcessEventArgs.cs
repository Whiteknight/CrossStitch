using System;

namespace CrossStitch.Stitch.Process.Core
{
    public class StitchProcessEventArgs : EventArgs
    {
        public StitchProcessEventArgs(string instanceId, bool isRunning, bool changeRequested)
        {
            InstanceId = instanceId;
            IsRunning = isRunning;
            ChangeRequested = changeRequested;
        }

        public string InstanceId { get; private set; }
        public bool IsRunning { get; set; }
        public bool ChangeRequested { get; set; }
    }
}