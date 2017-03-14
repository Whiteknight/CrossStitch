using Acquaintance;
using CrossStitch.Core.Messages.Core;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Core
{
    public class CoreModule : IModule
    {
        private readonly CrossStitchCore _core;
        private readonly IMessageBus _messageBus;
        private readonly SubscriptionCollection _subscriptions;

        public CoreModule(CrossStitchCore core, IMessageBus messageBus)
        {
            _core = core;
            _messageBus = messageBus;
            _subscriptions = new SubscriptionCollection(_messageBus);
        }

        public string Name => ModuleNames.Core;

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            string modules = "Core," + string.Join(",", _core.Modules.AddedModules);
            return new Dictionary<string, string>
            {
                { "Modules", modules },
                { "NodeId", _core.NodeId.ToString() },
            };
        }

        public void Start(CrossStitchCore core)
        {
            _subscriptions.Listen<ModuleStatusRequest, ModuleStatusResponse>(l => l
                .OnDefaultChannel()
                .Invoke(GetModuleStatus));
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private ModuleStatusResponse GetModuleStatus(ModuleStatusRequest arg)
        {
            if (arg.ModuleName == Name)
                return ModuleStatusResponse.Ok(Name, GetStatusDetails());
            var module = _core.Modules.Get(arg.ModuleName);
            if (module == null)
                return ModuleStatusResponse.NotFound(arg.ModuleName);
            var status = module.GetStatusDetails() ?? new Dictionary<string, string>();
            return ModuleStatusResponse.Ok(arg.ModuleName, status);
        }
    }
}
