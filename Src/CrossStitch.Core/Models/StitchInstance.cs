namespace CrossStitch.Core.Models
{
    public class StitchInstance : IDataEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }

        public string OwnerNodeId { get; set; }
        public string OwnerNodeName { get; set; }

        public StitchGroupName GroupName { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

        public InstanceStateType State { get; set; }
        public long LastHeartbeatReceived { get; set; }

        public StitchFullId FullId => new StitchFullId(OwnerNodeId, Id);
    }
}
