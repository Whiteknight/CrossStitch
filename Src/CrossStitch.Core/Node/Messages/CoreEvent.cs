namespace CrossStitch.Core.Node.Messages
{
    public class CoreEvent
    {
        public const string ChannelInitialized = "initialized";
        public const string ChannelNameChanged = "namechanged";
        public const string ChannelModuleAdded = "moduleadded";

        public CoreEvent(string nodeName, string data = null)
        {
            NodeName = nodeName;
            Data = data;
        }

        public string NodeName { get; }
        public string Data { get; }
    }
}
