using Acquaintance;
using CrossStitch.Core.Messages.Core;
using CrossStitch.Core.Modules;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class ModulesNancyModule : NancyModule
    {
        public ModulesNancyModule(IMessageBus messageBus)
            : base("/modules")
        {
            Get("/", _ =>
            {
                var request = new ModuleStatusRequest { ModuleName = ModuleNames.Core };
                return messageBus.Request<ModuleStatusRequest, ModuleStatusResponse>(request);
            });

            Get("/{ModuleName}/status", _ =>
            {
                var request = new ModuleStatusRequest { ModuleName = _.ModuleName.ToString() };
                return messageBus.Request<ModuleStatusRequest, ModuleStatusResponse>(request);
            });
        }
    }
}