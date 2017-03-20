using Acquaintance;
using CrossStitch.Http.NancyFx.Converters;
using Nancy.Bootstrapper;
using Nancy.Json;
using Nancy.Responses.Negotiation;

namespace CrossStitch.Http.NancyFx
{
    public class HttpModuleBootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private readonly IMessageBus _messageBus;

        public HttpModuleBootstrapper(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            JsonSettings.PrimitiveConverters.Add(new JsonConvertEnum());
            container.Register(_messageBus);
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                var processors = new[]
                {
                    typeof(JsonProcessor)
                };
                return NancyInternalConfiguration.WithOverrides(c =>
                {
                    c.ResponseProcessors = processors;
                });
            }
        }
    }
}
