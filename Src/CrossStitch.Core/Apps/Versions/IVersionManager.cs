using System.Collections.Generic;

namespace CrossStitch.Core.Apps.Versions
{
    public interface IVersionManager
    {
        string GetNextAvailableVersion(IEnumerable<string> versions);
    }
}