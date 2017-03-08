using CrossStitch.Core.Configuration;

namespace CrossStitch.Core
{
    public class NodeConfiguration
    {
        public static NodeConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<NodeConfiguration>("node.json");
        }
    }
}