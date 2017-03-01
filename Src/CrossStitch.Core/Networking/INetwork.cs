namespace CrossStitch.Core.Utility.Networking
{
    public interface INetwork
    {
        IReceiveChannel CreateReceiveChannel(bool allowMultipleClients);
        ISendChannel CreateSendChannel();
    }
}