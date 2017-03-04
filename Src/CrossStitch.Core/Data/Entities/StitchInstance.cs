﻿namespace CrossStitch.Core.Data.Entities
{
    public class StitchInstance : IDataEntity
    {
        public const string CreateEvent = "Create";

        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }

        public string Application { get; set; }
        public string Component { get; set; }
        public string Version { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

        public string FullName { get; set; }
        public string DirectoryPath { get; set; }
        public string ExecutableName { get; set; }
        public InstanceStateType State { get; set; }
        public int MissedHeartbeats { get; set; }
    }

    public enum InstanceStateType
    {
        // The stitch has started, but hasn't yet checked in
        Started,

        // The stitch is running
        Running,

        // The stitch could not be started
        Error,

        // The stitch has been stopped
        Stopped,

        // The stitch should be running, but it cannot be found.
        Missing
    }

    public enum InstanceRunModeType
    {
        //AppDomain,
        V1Process
    }

    public class InstanceAdaptorDetails
    {
        public InstanceRunModeType RunMode { get; set; }
    }
}