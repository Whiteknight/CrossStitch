using CrossStitch.Core.Models;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceResponse
    {
        public InstanceResponse()
        {
            Instances = new List<StitchInstance>();
        }

        public string Id { get; set; }
        public List<StitchInstance> Instances { get; set; }
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

        public static InstanceResponse Success(InstanceRequest request, StitchInstance instance = null, string data = null)
        {
            return Create(request, true, instance, data);
        }

        public static InstanceResponse Success(CreateInstanceRequest request, StitchInstance createdInstance, string data = null)
        {
            var response = new InstanceResponse
            {
                Id = createdInstance.Id,
                Data = data,
                IsSuccess = true
            };
            response.Instances.Add(createdInstance);
            return response;
        }

        public static InstanceResponse Create(InstanceRequest request, bool success, StitchInstance instance = null, string data = null)
        {
            var response = new InstanceResponse
            {
                IsSuccess = success,
                Id = request.Id,
                Data = data,
            };
            if (instance != null)
                response.Instances.Add(instance);
            return response;
        }
    }
}
