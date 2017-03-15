using Acquaintance;
using CrossStitch.Core.MessageBus;
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
        private readonly ModuleLog _log;

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
            _log = new ModuleLog(messageBus, Name);
        }

        public string Name => $"Http:{_httpConfiguration.Port}";

        public void Start()
        {
            _host.Start();
            _log.LogInformation("REST API Listening on port {0}", _httpConfiguration.Port);
        }

        public void Stop()
        {
            _host.Stop();
            _log.LogInformation("REST API stopped Listening on port {0}", _httpConfiguration.Port);
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { "ListeningPort", _httpConfiguration.Port.ToString() }
            };
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
