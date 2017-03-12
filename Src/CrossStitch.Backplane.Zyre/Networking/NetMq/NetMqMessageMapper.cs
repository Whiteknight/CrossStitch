using CrossStitch.Core.Utility;
using CrossStitch.Core.Utility.Serialization;
using NetMQ;
using System.Linq;
using System.Text;

namespace CrossStitch.Backplane.Zyre.Networking.NetMq
{
    public class NetMqMessageMapper : IMapper<NetMQMessage, MessageEnvelope>, IMapper<MessageEnvelope, NetMQMessage>
    {
        private readonly ISerializer _serializer;
        private readonly MessageEnvelopeBuilderFactory _envelopeFactory;

        public NetMqMessageMapper(ISerializer serializer, MessageEnvelopeBuilderFactory envelopeFactory)
        {
            _serializer = serializer;
            _envelopeFactory = envelopeFactory;
        }

        public MessageEnvelope Map(NetMQMessage source)
        {
            var envelope = _envelopeFactory.CreateNew().Build();
            envelope.Header = _serializer.Deserialize<MessageHeader>(source.Pop().Buffer);
            if (envelope.Header.PayloadType == MessagePayloadType.Raw)
                envelope.RawFrames = source.Select(f => f.ToByteArray()).ToList();
            else if (envelope.Header.PayloadType == MessagePayloadType.Object)
            {
                var bytes = source.FirstOrDefault()?.ToByteArray();
                if (bytes != null)
                    envelope.PayloadObject = _serializer.DeserializeObject(bytes);
            }
            else if (envelope.Header.PayloadType == MessagePayloadType.CommandString)
            {
                envelope.CommandStrings = source
                    .Select(f => f.ToByteArray())
                    .Select(b => Encoding.Unicode.GetString(b))
                    .ToList();
            }
            return envelope;
        }

        public NetMQMessage Map(MessageEnvelope envelope)
        {
            var message = new NetMQMessage();
            byte[] headerFrame = _serializer.Serialize(envelope.Header);
            message.Append(headerFrame);
            if (envelope.Header.PayloadType == MessagePayloadType.CommandString)
            {
                foreach (var command in envelope.CommandStrings)
                {
                    var bytes = Encoding.Unicode.GetBytes(command);
                    message.Append(bytes);
                }
            }
            if (envelope.Header.PayloadType == MessagePayloadType.Object)
            {
                var bytes = _serializer.Serialize(envelope.PayloadObject);
                message.Append(bytes);
            }
            else if (envelope.Header.PayloadType == MessagePayloadType.Raw)
            {
                foreach (var payload in envelope.RawFrames)
                    message.Append(payload);
            }
            return message;
        }
    }
}