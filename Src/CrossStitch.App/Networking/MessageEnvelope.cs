using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.App.Utility.Extensions;

namespace CrossStitch.App.Networking
{
    public class MessageEnvelope
    {
        public const string SendEventName = "Send";
        public const string ReceiveEventName = "Receive";

        public MessageHeader Header { get; set; }
        public List<string> CommandStrings { get; set; }
        public List<object> PayloadObjects { get; set; }
        public List<byte[]> RawFrames { get; set; }

        public static MessageEnvelopeBuilder CreateNew()
        {
            return new MessageEnvelopeBuilder(new MessageEnvelope());
        }

        private MessageEnvelope()
        {
            Header = new MessageHeader {
                PayloadType = MessagePayloadType.None
            };
        }

        public MessageEnvelope CreateEmptyResponse()
        {
            return CreateNew().ResponseTo(this).Envelope;
        }

        public MessageEnvelope CreateFailureResponse()
        {
            return CreateNew().FailureResponseTo(this).Envelope;
        }

        public class MessageEnvelopeBuilder
        {
            private readonly MessageEnvelope _envelope;

            public MessageEnvelopeBuilder(MessageEnvelope envelope)
            {
                _envelope = envelope;
            }

            public MessageEnvelope Envelope
            {
                get { return _envelope; }
            }

            public MessageEnvelopeBuilder ResponseTo(MessageEnvelope request)
            {
                _envelope.Header.FromId = request.Header.ToId;
                _envelope.Header.FromType = request.Header.ToType;
                _envelope.Header.ToId = request.Header.FromId;
                _envelope.Header.ToType = request.Header.FromType;
                _envelope.Header.MessageId = request.Header.MessageId;
                _envelope.Header.ZoneName = request.Header.ZoneName;
                _envelope.Header.PayloadType = MessagePayloadType.SuccessResponse;
                return this;
            }

            public MessageEnvelopeBuilder FailureResponseTo(MessageEnvelope request)
            {
                _envelope.Header.PayloadType = MessagePayloadType.FailureResponse;
                return ResponseTo(request);
            }

            public MessageEnvelopeBuilder WithObjectPayload(IEnumerable<object> objects)
            {
                _envelope.Header.PayloadType = MessagePayloadType.Object;
                _envelope.PayloadObjects = objects.OrEmptyIfNull().ToList();
                return this;
            }

            public MessageEnvelopeBuilder WithObjectPayload(object payload)
            {
                _envelope.Header.PayloadType = MessagePayloadType.Object;
                _envelope.PayloadObjects = new List<object> { payload };
                return this;
            }

            public MessageEnvelopeBuilder WithCommandStrings(IEnumerable<string> commandStrings)
            {
                _envelope.Header.PayloadType = MessagePayloadType.CommandString;
                if (_envelope.CommandStrings == null)
                    _envelope.CommandStrings = new List<string>();
                _envelope.CommandStrings.AddRange(commandStrings.OrEmptyIfNull());
                return this;
            }

            public MessageEnvelopeBuilder WithCommandString(string commandString)
            {
                _envelope.Header.PayloadType = MessagePayloadType.CommandString;
                if (_envelope.CommandStrings == null)
                    _envelope.CommandStrings = new List<string>();
                _envelope.CommandStrings.Add(commandString);
                return this;
            }

            public MessageEnvelopeBuilder WithRawFrames(IEnumerable<byte[]> frames)
            {
                _envelope.Header.PayloadType = MessagePayloadType.Raw;
                _envelope.RawFrames = frames.OrEmptyIfNull().ToList();
                return this;
            }

            public MessageEnvelopeBuilder ToCluster()
            {
                _envelope.Header.ToType = TargetType.Cluster;
                return this;
            }

            public MessageEnvelopeBuilder ToNode(Guid nodeId)
            {
                _envelope.Header.ToType = TargetType.Node;
                _envelope.Header.ToId = nodeId;
                return this;
            }

            public MessageEnvelopeBuilder ToAppInstance(Guid appInstanceId)
            {
                _envelope.Header.ToType = TargetType.AppInstance;
                _envelope.Header.ToId = appInstanceId;
                return this;
            }

            public MessageEnvelopeBuilder ToZone(string zoneName)
            {
                _envelope.Header.ToType = TargetType.Zone;
                _envelope.Header.ZoneName = zoneName;
                return this;
            }

            public MessageEnvelopeBuilder Local()
            {
                _envelope.Header.ToType = TargetType.Local;
                _envelope.Header.FromType = TargetType.Local;
                return this;
            }

            public MessageEnvelopeBuilder FromNode(Guid nodeId)
            {
                _envelope.Header.FromType = TargetType.Node;
                _envelope.Header.FromId = nodeId;
                return this;
            }

            public MessageEnvelopeBuilder WithEventName(string eventName)
            {
                _envelope.Header.EventName = eventName;
                return this;
            }
        }
    }
}