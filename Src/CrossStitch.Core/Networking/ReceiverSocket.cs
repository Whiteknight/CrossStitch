using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.Core.Networking
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(List<byte[]> frames)
        {
            Frames = frames;
        }

        public List<byte[]> Frames { get; private set; }
    }
    public class ReceiverSocket : IDisposable
    {
        private ResponseSocket _serverSocket;
        private NetMQPoller _poller;
        public int Port { get; private set; }

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

        private void OnMessageReceived(List<byte[]> frames)
        {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, new MessageReceivedEventArgs(frames));
        }

        private void ServerSocketOnReceiveReady(object sender, NetMQSocketEventArgs netMqSocketEventArgs)
        {
            List<byte[]> frames = new List<byte[]>();
            netMqSocketEventArgs.Socket.ReceiveMultipartBytes(ref frames);
            if (frames.Any())
                OnMessageReceived(frames);
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}
