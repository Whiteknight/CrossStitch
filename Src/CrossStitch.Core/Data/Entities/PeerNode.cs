using System;

namespace CrossStitch.Core.Data.Entities
{
    public class PeerNode : IDataEntity
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
    }
}
