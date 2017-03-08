using CrossStitch.Core.Utility.Serialization;

namespace CrossStitch.Backplane.Zyre.Networking.NetMq
{
    public class NetMqNetwork : INetwork
    {
        private readonly NetMqMessageMapper _mapper;

        public NetMqNetwork()
        {
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        public IReceiveChannel CreateReceiveChannel(bool allowMultipleClients)
        {
            if (allowMultipleClients)
                return new MultiReceiveChannel(_mapper);
            else
                return new SingleReceiveChannel(_mapper);
        }

        public ISendChannel CreateSendChannel()
        {
            return new SendChannel(_mapper);
        }
    }
}
