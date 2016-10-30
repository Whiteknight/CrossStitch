namespace CrossStitch.Core.Data.Entities
{
    public class PeerNode : IDataEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }
    }
}
