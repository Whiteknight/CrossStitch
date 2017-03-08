namespace CrossStitch.Core.Messages.Backplane
{
    public class BackplaneEvent
    {
        public const string ChannelNetworkIdChanged = "NetworkIdChanged";

        public string Data { get; set; }
    }
}
