using System;

namespace CrossStitch.Core.Data
{
    public interface IDataEntity
    {
        Guid Id { get; set; }
        long Version { get; set; }
    }
}
