using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class EnrichedInstanceRequest : InstanceRequest
    {
        public EnrichedInstanceRequest(StitchInstance instance)
        {
            Id = instance.Id;
            StitchInstance = instance;
        }

        public EnrichedInstanceRequest(string id, StitchInstance instance)
        {
            Id = id;
            StitchInstance = instance;
        }

        public EnrichedInstanceRequest(InstanceRequest rawRequest, StitchInstance instance)
        {
            Id = rawRequest.Id;
            DataId = rawRequest.DataId;
            StitchInstance = instance;
        }

        public StitchInstance StitchInstance { get; set; }

        public bool IsValid()
        {
            return StitchInstance != null;
        }
    }
}