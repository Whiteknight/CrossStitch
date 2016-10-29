using System;

namespace CrossStitch.Core.Data.Entities
{
    public class Instance : IDataEntity
    {
        public Guid Id { get; set; }
        public long Version { get; set; }

        public Guid ApplicationId { get; set; }
        public Guid ComponentId { get; set; }
        public Guid VersionId { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

    }

    public class InstanceAdaptorDetails
    {

    }
}
