using System;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceResponse
    {
        public string Id { get; set; }
        public StitchInstance Instance { get; set; }
        public bool IsSuccess { get; set; }
        public string Data { get; set; }
        public Exception Exception { get; set; }

        public static InstanceResponse Failure(InstanceRequest request)
        {
            return Create(request, false);
        }

        public static InstanceResponse Failure(InstanceRequest request, Exception e)
        {
            var response = Create(request, false);
            response.Exception = e;
            return response;
        }

        public static InstanceResponse Success(InstanceRequest request, string data = null)
        {
            return Create(request, true, data);
        }

        public static InstanceResponse Create(InstanceRequest request, bool success, string data = null)
        {
            return new InstanceResponse
            {
                IsSuccess = success,
                Id = request.Id,
                Instance = request.Instance,
                Data = data
            };
        }
    }
}
