using System;
using CrossStitch.Core.Events;
using CrossStitch.Core.Utility.Serialization;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.Core.Utility.Networking.NetMq
{
    public class MultiReceiveChannel : IReceiveChannel
    {
        private RouterSocket _serverSocket;
        private NetMQPoller _poller;
        private readonly NetMqMessageMapper _mapper;

        public MultiReceiveChannel()
        {
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        public MultiReceiveChannel(NetMqMessageMapper mapper)
        {
            _mapper = mapper;
        }

        public int Port { get; private set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void StartListening(string host = null)
        {
            if (_serverSocket != null)
                throw new Exception("Socket is already listening");
            string connection = $"tcp://{host ?? "*"}";
            _serverSocket = new RouterSocket();
            Port = _serverSocket.BindRandomPort(connection);
            _serverSocket.ReceiveReady += ServerSocketOnReceiveReady;
            _poller = new NetMQPoller { _serverSocket };
            _poller.RunAsync();
        }

        public void StartListening(string host, int port)
        {
            if (_serverSocket != null)
                throw new Exception("Socket is already listening");
            string connection = $"tcp://{host ?? "*"}:{port}";
            _serverSocket = new RouterSocket();
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

        public void Dispose()
        {
            StopListening();
        }

        private void ServerSocketOnReceiveReady(object sender, NetMQSocketEventArgs netMqSocketEventArgs)
        {
            var message = netMqSocketEventArgs.Socket.ReceiveMultipartMessage();
            var identityFrame = message.Pop();
            var received = _mapper.Map(message);
            var response = received.CreateEmptyResponse();
            var eventArgs = new MessageReceivedEventArgs(received, response);
            try
            {
                MessageReceived.Raise(this, eventArgs);
            }
            catch
            {
                eventArgs.HandledOk = false;
            }
            if (!eventArgs.HandledOk)
                response = received.CreateFailureResponse();
            var outMessage = _mapper.Map(response);
            outMessage.Push(identityFrame);
            netMqSocketEventArgs.Socket.SendMultipartMessage(outMessage);
        }
    }
}