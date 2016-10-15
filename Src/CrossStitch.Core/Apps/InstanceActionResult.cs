using System;

namespace CrossStitch.Core.Apps
{
    public class InstanceActionResult
    {
        public Guid InstanceId { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }
}