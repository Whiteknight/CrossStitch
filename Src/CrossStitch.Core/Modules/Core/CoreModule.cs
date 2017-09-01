using Acquaintance;
using CrossStitch.Core.Messages.Core;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Core
{
    public class CoreModule : IModule
    {
        private readonly CrossStitchCore _core;
        private readonly SubscriptionCollection _subscriptions;
        private readonly CoreService _service;

        public CoreModule(CrossStitchCore core, IMessageBus messageBus)
        {
            _core = core;
            _subscriptions = new SubscriptionCollection(messageBus);
            _service = new CoreService(core);
        }

        public string Name => ModuleNames.Core;

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            string modules = "Core," + string.Join(",", _core.Modules.AddedModules);
            return new Dictionary<string, string>
            {
                { "Modules", modules },
                { "NodeId", _core.NodeId },
            };
        }

        public void Start()
        {
            _subscriptions.Listen<ModuleStatusRequest, ModuleStatusResponse>(l => l
                .WithDefaultTopic()
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
            var status = _service.GetModuleStatusDetails(arg.ModuleName);
            if (status == null)
                return ModuleStatusResponse.NotFound(arg.ModuleName);
            return ModuleStatusResponse.Ok(arg.ModuleName, status);
        }
    }
}
