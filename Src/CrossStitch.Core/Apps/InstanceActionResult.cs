using System;

namespace CrossStitch.Core.Apps
{
    public class InstanceActionResult
    {
        public Guid InstanceId { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }

        public static InstanceActionResult Failure()
        {
            return new InstanceActionResult { IsSuccess = false };
        }

        public static InstanceActionResult Failure(Exception e)
        {
            return new InstanceActionResult {
                IsSuccess = false,
                Exception = e
            };
        }
    }
}