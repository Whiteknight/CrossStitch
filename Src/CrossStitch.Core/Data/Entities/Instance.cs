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

        public string FullName { get; set; }
        public string DirectoryPath { get; set; }
        public string ExecutableName { get; set; }
        public string ApplicationClassName { get; set; }
        public InstanceStateType State { get; set; }
    }

    public enum InstanceStateType
    {
        Started,
        Running,
        Error,
        Stopped
    }

    public enum InstanceRunModeType
    {
        AppDomain,
        Process
    }

    public class InstanceAdaptorDetails
    {
        public InstanceRunModeType RunMode { get; set; }
    }
}
