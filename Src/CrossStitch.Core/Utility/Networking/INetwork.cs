namespace CrossStitch.App.Networking
{
    public interface INetwork
    {
        IReceiveChannel CreateReceiveChannel(bool allowMultipleClients);
        ISendChannel CreateSendChannel();
    }
}