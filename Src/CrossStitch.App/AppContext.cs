using System;
using CrossStitch.App.Networking;

namespace CrossStitch.App
{
    public class AppContext : IDisposable
    {
        private readonly int _communicationPort;
        private readonly ISendChannel _sender;
        private readonly IReceiveChannel _receiver;

        public AppContext(INetwork network, int communicationPort)
        {
            _communicationPort = communicationPort;
            _sender = network.CreateSendChannel();
            _receiver = network.CreateReceiveChannel(false);
            _receiver.MessageReceived += MessageReceived;
        }

        internal bool Initialize()
        {
            _sender.Connect("127.0.0.1", _communicationPort);
            _receiver.StartListening("127.0.0.1");

            var envelope = MessageEnvelope.CreateNew()
                .Local()
                .WithCommandString("App Instance Initialize")
                .WithCommandString("ReceivePort=" + _receiver.Port)
                .Envelope;
            
            return _sender.SendMessage(envelope);
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public bool SendMessage(MessageEnvelope envelope)
        {
            return _sender.SendMessage(envelope);
        }

        public void Dispose()
        {
            _sender.Disconnect();
            _sender.Dispose();

            _receiver.StopListening();
            _receiver.Dispose();
        }
    }
}