namespace CrossStitch.Core.Data.Entities
{
    public class Instance : IDataEntity
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
        //AppDomain,
        V1Process
    }

    public class InstanceAdaptorDetails
    {
        public InstanceRunModeType RunMode { get; set; }
    }
}
