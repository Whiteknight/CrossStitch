namespace CrossStitch.Core.Networking
{
    public interface INetwork
    {
        IReceiveChannel CreateReceiveChannel(bool allowMultipleClients);
        ISendChannel CreateSendChannel();
    }
}