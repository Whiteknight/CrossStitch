using System.Linq;
using System.Text;
using CrossStitch.Core.Utility;
using CrossStitch.Core.Utility.Serialization;
using NetMQ;

namespace CrossStitch.Backplane.Zyre.Networking.NetMq
{
    public class NetMqMessageMapper : IMapper<NetMQMessage, MessageEnvelope>, IMapper<MessageEnvelope, NetMQMessage>
    {
        private readonly ISerializer _serializer;

        public NetMqMessageMapper(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public MessageEnvelope Map(NetMQMessage source)
        {
            var envelope = MessageEnvelope.CreateNew().Envelope;
            envelope.Header = _serializer.Deserialize<MessageHeader>(source.Pop().Buffer);
            if (envelope.Header.PayloadType == MessagePayloadType.Raw)
                envelope.RawFrames = source.Select(f => f.ToByteArray()).ToList();
            else if (envelope.Header.PayloadType == MessagePayloadType.Object)
            {
                envelope.PayloadObjects = source
                    .Select(f => f.ToByteArray())
                    .Select(b => _serializer.DeserializeObject(b))
                    .ToList();
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
            NetMQMessage message = new NetMQMessage();
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
            else if (envelope.Header.PayloadType == MessagePayloadType.Object)
            {
                foreach (var payload in envelope.PayloadObjects)
                {
                    var bytes = _serializer.Serialize(payload);
                    message.Append(bytes);
                }
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