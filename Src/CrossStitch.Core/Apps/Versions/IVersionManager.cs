using System.Collections.Generic;

namespace CrossStitch.Core.Apps
{
    public interface IVersionManager
    {
        string GetNextAvailableVersion(IEnumerable<string> versions);
    }
}