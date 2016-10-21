using System;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.App.Networking.NetMq
{
    public class SendChannel : ISendChannel
    {
        private readonly NetMqMessageMapper _mapper;

        private RequestSocket _clientSocket;

        public SendChannel()
        {
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        public SendChannel(NetMqMessageMapper mapper)
        {
            _mapper = mapper;
        }

        public bool Connect(string host, int port)
        {
            _clientSocket = new RequestSocket();
            string connection = string.Format("tcp://{0}:{1}", host, port);
            _clientSocket.Connect(connection);
            return true;
        }

        public bool SendMessage(MessageEnvelope envelope)
        {
            lock (this)
            {
                var message = _mapper.Map(envelope);
                _clientSocket.SendMultipartMessage(message);
                string response;
                bool ok = _clientSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out response);
                return ok;
            }
        }

        public void Disconnect()
        {
            if (_clientSocket == null)
                return;
            _clientSocket.Dispose();
            _clientSocket = null;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}