﻿using CrossStitch.Core.Configuration;

namespace CrossStitch.Core.Http
{
    public class HttpConfiguration
    {
        public static HttpConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<HttpConfiguration>("http.json");
        }

        public int Port { get; set; }
    }
}