using System;
using CrossStitch.App.Events;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.App.Networking
{
    public class ReceiveChannel : IDisposable
    {
        private ResponseSocket _serverSocket;
        private NetMQPoller _poller;
        public int Port { get; private set; }
        private readonly NetMqMessageMapper _mapper;

        public ReceiveChannel()
        {
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        public ReceiveChannel(NetMqMessageMapper mapper)
        {
            _mapper = mapper;
        }

        public void StartListening(string host = null)
        {
            if (_serverSocket != null)
                throw new Exception("Socket is already listening");
            string connection = string.Format("tcp://{0}", host ?? "*");
            _serverSocket = new ResponseSocket();
            Port = _serverSocket.BindRandomPort(connection);
            _serverSocket.ReceiveReady += ServerSocketOnReceiveReady;
            _poller = new NetMQPoller { _serverSocket };
            _poller.RunAsync();
        }

        public void StartListening(string host, int port)
        {
            if (_serverSocket != null)
                throw new Exception("Socket is already listening");
            string connection = string.Format("tcp://{0}:{1}", host ?? "*", port);
            _serverSocket = new ResponseSocket();
            _serverSocket.Bind(connection);
            Port = port;
            _serverSocket.ReceiveReady += ServerSocketOnReceiveReady;
            _poller = new NetMQPoller { _serverSocket };
            _poller.RunAsync();
        }

        public void StopListening()
        {
            if (_serverSocket == null)
                return;
            _poller.Stop();
            _poller = null;
            _serverSocket.Dispose();
            _serverSocket = null;
        }

        public EventHandler<MessageReceivedEventArgs> MessageReceived;

        private void ServerSocketOnReceiveReady(object sender, NetMQSocketEventArgs netMqSocketEventArgs)
        {
            var message = netMqSocketEventArgs.Socket.ReceiveMultipartMessage();
            var envelope = _mapper.Map(message);
            MessageReceived.Raise(this, new MessageReceivedEventArgs(envelope));
            netMqSocketEventArgs.Socket.SendFrame("OK");
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}
