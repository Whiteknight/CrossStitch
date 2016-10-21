using System;
using System.Collections.Generic;
using CrossStitch.App.Networking;

namespace CrossStitch.App
{
    public class AppContext : IDisposable
    {
        private readonly INetwork _network;
        private readonly int _communicationPort;
        private readonly ISendChannel _sender;
        private readonly IReceiveChannel _receiver;

        public AppContext(INetwork network, int communicationPort)
        {
            _network = network;
            _communicationPort = communicationPort;
            _sender = network.CreateSendChannel();
            _receiver = network.CreateReceiveChannel(false);
            _receiver.MessageReceived += MessageReceived;
        }

        internal bool Initialize()
        {
            _sender.Connect("127.0.0.1", _communicationPort);
            _receiver.StartListening("127.0.0.1");

            var envelope = new MessageEnvelope {
                Header = new MessageHeader {
                    FromType = TargetType.Local,
                    ToType = TargetType.Local,
                    PayloadType = MessagePayloadType.CommandString
                },
                CommandStrings = new List<string> {
                    "App Instance Initialize",
                    "ReceivePort=" + _receiver.Port
                }
            };
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