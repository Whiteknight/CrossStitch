using CrossStitch.Core.Models;
using System;

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

        public static InstanceResponse Failure(CreateInstanceRequest request)
        {
            return new InstanceResponse
            {
                IsSuccess = false
            };
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
                Data = data
            };
        }

        public static InstanceResponse Create(EnrichedInstanceRequest request, bool success, string data = null)
        {
            return new InstanceResponse
            {
                IsSuccess = success,
                Id = request.Id,
                Instance = request.StitchInstance,
                Data = data
            };
        }
    }
}
