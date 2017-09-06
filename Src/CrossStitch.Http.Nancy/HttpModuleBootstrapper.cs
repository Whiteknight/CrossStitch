using System;
using Acquaintance;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses.Negotiation;

namespace CrossStitch.Http.NancyFx
{
    public class HttpModuleBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IMessageBus _messageBus;

        public HttpModuleBootstrapper(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            container.Register(_messageBus);
        }

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
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
