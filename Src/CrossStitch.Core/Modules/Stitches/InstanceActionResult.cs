using System;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
    public class InstanceActionResult
    {
        public string InstanceId { get; set; }
        public StitchInstance StitchInstance { get; set; }
        public bool Success { get; set; }
        public Exception Exception { get; set; }
        public bool Found { get; set; }

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