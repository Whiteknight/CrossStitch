namespace CrossStitch.Backplane.Zyre.Networking
{
    public interface INetwork
    {
        IReceiveChannel CreateReceiveChannel(bool allowMultipleClients);
        ISendChannel CreateSendChannel();
    }
}