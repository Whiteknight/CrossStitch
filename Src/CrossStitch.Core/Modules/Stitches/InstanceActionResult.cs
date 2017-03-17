using CrossStitch.Core.Models;
using System;

namespace CrossStitch.Core.Modules.Stitches
{
    public class InstanceActionResult
    {
        public string InstanceId { get; set; }
        public StitchInstance StitchInstance { get; set; }
        public bool Success { get; set; }
        public Exception Exception { get; set; }
        public bool Found { get; set; }

        public static InstanceActionResult NotFound(string instanceId)
        {
            return new InstanceActionResult
            {
                InstanceId = instanceId,
                Success = false,
                Found = false
            };
        }

        public static InstanceActionResult Result(string instanceId, bool ok, StitchInstance instance = null)
        {
            return new InstanceActionResult
            {
                Found = true,
                InstanceId = instanceId,
                Success = ok,
                StitchInstance = instance
            };
        }

        public static InstanceActionResult Failure(string instanceId, bool found, StitchInstance instance, Exception e)
        {
            return new InstanceActionResult
            {
                Success = false,
                Found = found,
                InstanceId = instanceId,
                Exception = e,
                StitchInstance = instance
            };
        }

        public static InstanceActionResult Failure(string instanceId, bool found, Exception e)
        {
            return new InstanceActionResult
            {
                Success = false,
                Found = found,
                InstanceId = instanceId,
                Exception = e
            };
        }
    }
}