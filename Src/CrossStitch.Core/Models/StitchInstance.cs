using CrossStitch.Stitch.Utility;

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

        public InstanceStateType State { get; set; }
        public long LastHeartbeatReceived { get; set; }

        public StitchFullId FullId => new StitchFullId(OwnerNodeId, Id);

        public string AdaptorData { get; set; }

        public void SetAdaptorDataObject<T>(T data)
        {
            AdaptorData = data == null ? string.Empty : JsonUtility.Serialize(data);
        }

        public T GetAdaptorDataObject<T>()
        {
            return JsonUtility.Deserialize<T>(AdaptorData ?? string.Empty);
        }
    }
}
