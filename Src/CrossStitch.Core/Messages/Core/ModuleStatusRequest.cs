using System.Collections.Generic;

namespace CrossStitch.Core.Messages.Core
{
    public class ModuleStatusRequest
    {
        public string ModuleName { get; set; }
    }

    public class ModuleStatusResponse
    {
        public string ModuleName { get; set; }
        public bool Found { get; set; }
        public IReadOnlyDictionary<string, string> StatusValues { get; set; }

        public static ModuleStatusResponse NotFound(string name)
        {
            return new ModuleStatusResponse
            {
                ModuleName = name,
                Found = false
            };
        }

        public static ModuleStatusResponse Ok(string name, IReadOnlyDictionary<string, string> status)
        {
            return new ModuleStatusResponse
            {
                ModuleName = name,
                Found = true,
                StatusValues = status
            };
        }
    }
}
