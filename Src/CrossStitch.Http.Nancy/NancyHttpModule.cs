using System.Net;
using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy.Owin;

namespace CrossStitch.Http.NancyFx
{
    // TODO: We should try to detect which modules are running. We shouldn't load the 
    // StitchesNancyModule, for example, if the Stitches module isn't loaded.
    public class NancyHttpModule : IModule
    {
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ModuleLog _log;
        private readonly IWebHost _host;

        public NancyHttpModule(IMessageBus messageBus, HttpConfiguration httpConfiguration = null)
        {
            _httpConfiguration = httpConfiguration ?? HttpConfiguration.GetDefault();
            _host = new WebHostBuilder()
                .Configure(cfg => cfg
                    .UseOwin(x => x
                        .UseNancy(new NancyOptions {
                            Bootstrapper = new HttpModuleBootstrapper(messageBus)
                        })))
                .UseKestrel(o => o.Listen(IPAddress.Any, _httpConfiguration.Port))
                .Build();

            
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
            _host.StopAsync().Wait();
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
