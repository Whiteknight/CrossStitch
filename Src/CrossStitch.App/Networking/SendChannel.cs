using System;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.App.Networking
{
    public class SendChannel : IDisposable
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

        public bool Connect(string connectionString)
        {
            _clientSocket = new RequestSocket();
            _clientSocket.Connect(connectionString);
            return true;
        }

        public bool SendMessage(MessageEnvelope envelope)
        {
            var message = _mapper.Map(envelope);
            _clientSocket.SendMultipartMessage(message);
            string response;
            bool ok = _clientSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out response);
            return ok;
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