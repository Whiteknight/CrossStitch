using System;

namespace CrossStitch.Core.Apps
{
    public class AppStartedEventArgs : EventArgs
    {
        public AppStartedEventArgs(Guid instanceId)
        {
            InstanceId = instanceId;
        }

        public Guid InstanceId { get; private set; }
    }
}