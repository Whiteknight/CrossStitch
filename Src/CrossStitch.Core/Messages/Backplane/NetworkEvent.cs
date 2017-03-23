namespace CrossStitch.Core.Messages.Backplane
{
    public class BackplaneEvent
    {
        public const string ChannelNetworkIdChanged = "NetworkIdChanged";
        public const string ChannelSetZones = "SetZones";

        public string Data { get; set; }
    }
}
