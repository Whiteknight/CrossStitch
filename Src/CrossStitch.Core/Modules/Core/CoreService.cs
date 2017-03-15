using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Core
{
    public class CoreService
    {
        private readonly CrossStitchCore _core;

        public CoreService(CrossStitchCore core)
        {
            _core = core;
        }

        public IReadOnlyDictionary<string, string> GetModuleStatusDetails(string name)
        {
            var module = _core.Modules.Get(name);
            if (module == null)
                return null;
            var status = module.GetStatusDetails() ?? new Dictionary<string, string>();
            return status;
        }
    }
}
