using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches.Versions
{
    public interface IVersionManager
    {
        string GetNextAvailableVersion(IEnumerable<string> versions);
    }
}