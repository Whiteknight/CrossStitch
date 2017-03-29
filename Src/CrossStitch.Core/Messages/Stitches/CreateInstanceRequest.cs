using System.Collections.Generic;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class LocalCreateInstanceRequest
    {
        public string Name { get; set; }

        public StitchGroupName GroupName { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

        public int NumberOfInstances { get; set; }

        public bool IsValid()
        {
            return NumberOfInstances > 0
                && GroupName != null
                && GroupName.IsValid()
                && GroupName.IsVersionGroup()
                && Adaptor != null
                && !string.IsNullOrEmpty(Name);
        }
    }

    public class CreateInstanceRequest : LocalCreateInstanceRequest
    {
        public bool LocalOnly { get; set; }
        public string JobId { get; set; }
        public string TaskId { get; set; }

        public bool ReceiptRequested => !string.IsNullOrEmpty(JobId) && !string.IsNullOrEmpty(TaskId);
    }

    public class LocalCreateInstanceResponse
    {
        public LocalCreateInstanceResponse()
        {
            CreatedIds = new List<string>();
        }

        public bool IsSuccess { get; set; }
        public List<string> CreatedIds { get; set; }
    }

    public class CreateInstanceResponse : LocalCreateInstanceResponse
    {
        public CreateInstanceResponse()
        {
        }

        public CreateInstanceResponse(LocalCreateInstanceResponse inner)
        {
            IsSuccess = inner.IsSuccess;
            CreatedIds = inner.CreatedIds;
        }

        public string JobId { get; set; }
    }
}