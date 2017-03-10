using Acquaintance;
using CrossStitch.Core;
using CrossStitch.Core.Modules;
using Nancy.Hosting.Self;
using System;

namespace CrossStitch.Http.NancyFx
{
    // TODO: We should try to detect which modules are running. We shouldn't load the 
    // StitchesNancyModule, for example, if the Stitches module isn't loaded.
    public class NancyHttpModule : IModule
    {
        private readonly NancyHost _host;
        private readonly HttpConfiguration _httpConfiguration;

        public NancyHttpModule(HttpConfiguration httpConfiguration, IMessageBus messageBus)
        {
            var bootstrapper = new HttpModuleBootstrapper(messageBus);
            var hostConfig = new HostConfiguration
            {
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                }
            };
            _host = new NancyHost(new Uri("http://localhost:" + httpConfiguration.Port), bootstrapper, hostConfig);
            _httpConfiguration = httpConfiguration;
        }

        public string Name => $"Http:{_httpConfiguration.Port}";

        public void Start(CrossStitchCore core)
        {
            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
