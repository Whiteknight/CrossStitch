﻿using Acquaintance;
using CrossStitch.Core;
using CrossStitch.Core.Http;
using CrossStitch.Core.Node;
using Nancy.Hosting.Self;
using System;

namespace CrossStitch.Http.NancyFx
{
    public class NancyHttpModule : IModule
    {
        private readonly NancyHost _host;

        public NancyHttpModule(HttpConfiguration configuration, IMessageBus messageBus)
        {
            var bootstrapper = new HttpModuleBootstrapper(messageBus);
            var hostConfig = new HostConfiguration
            {
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                }
            };
            _host = new NancyHost(new Uri("http://localhost:" + configuration.Port), bootstrapper, hostConfig);
        }

        public string Name => "Http";

        public void Start(RunningNode context)
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