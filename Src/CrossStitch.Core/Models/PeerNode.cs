using CrossStitch.Core.Modules.Data;

namespace CrossStitch.Core.Models
{
    public class PeerNode : IDataEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }
    }
}
