using System;
using CrossStitch.Core.Data.Entities;

namespace CrossStitch.Core.Modules.Stitches
{
    public class InstanceActionResult
    {
        public string InstanceId { get; set; }
        public Instance Instance { get; set; }
        public bool Success { get; set; }
        public Exception Exception { get; set; }

        public static InstanceActionResult Failure()
        {
            return new InstanceActionResult { Success = false };
        }

        public static InstanceActionResult Failure(Exception e)
        {
            return new InstanceActionResult
            {
                Success = false,
                Exception = e
            };
        }
    }
}