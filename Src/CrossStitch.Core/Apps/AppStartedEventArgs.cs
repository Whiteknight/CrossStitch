using System;

namespace CrossStitch.Core.Apps
{
    public class AppStartedEventArgs : EventArgs
    {
        public AppStartedEventArgs(string instanceId)
        {
            InstanceId = instanceId;
        }

        public string InstanceId { get; private set; }
    }
}