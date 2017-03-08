namespace CrossStitch.Core.Messages
{
    public class CoreEvent
    {
        public const string ChannelInitialized = "initialized";
        public const string ChannelNetworkNodeIdChanged = "NetworkNodeIdChanged";
        public const string ChannelModuleAdded = "moduleadded";

        public CoreEvent(string data = null)
        {
            Data = data;
        }

        public string Data { get; }
    }
}
