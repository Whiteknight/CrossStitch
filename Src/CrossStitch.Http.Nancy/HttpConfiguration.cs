using CrossStitch.Core.Configuration;

namespace CrossStitch.Http.NancyFx
{
    public class HttpConfiguration : IModuleConfiguration
    {
        public static HttpConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<HttpConfiguration>("http.json");
        }

        public int Port { get; set; }
        public void ValidateAndSetDefaults()
        {
            if (Port <= 0)
                Port = 8080;
        }
    }
}
