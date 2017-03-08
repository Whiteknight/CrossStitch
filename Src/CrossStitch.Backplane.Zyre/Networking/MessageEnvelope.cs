using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Utility.Extensions;

namespace CrossStitch.Backplane.Zyre.Networking
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
            Header = new MessageHeader
            {
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
            public MessageEnvelopeBuilder(MessageEnvelope envelope)
            {
                Envelope = envelope;
            }

            public MessageEnvelope Envelope { get; }

            public MessageEnvelopeBuilder ResponseTo(MessageEnvelope request)
            {
                Envelope.Header.FromId = request.Header.ToId;
                Envelope.Header.FromType = request.Header.ToType;
                Envelope.Header.ToId = request.Header.FromId;
                Envelope.Header.ToType = request.Header.FromType;
                Envelope.Header.MessageId = request.Header.MessageId;
                Envelope.Header.ZoneName = request.Header.ZoneName;
                Envelope.Header.PayloadType = MessagePayloadType.SuccessResponse;
                return this;
            }

            public MessageEnvelopeBuilder FailureResponseTo(MessageEnvelope request)
            {
                Envelope.Header.PayloadType = MessagePayloadType.FailureResponse;
                return ResponseTo(request);
            }

            public MessageEnvelopeBuilder WithObjectPayload(IEnumerable<object> objects)
            {
                Envelope.Header.PayloadType = MessagePayloadType.Object;
                Envelope.PayloadObjects = objects.OrEmptyIfNull().ToList();
                return this;
            }

            public MessageEnvelopeBuilder WithObjectPayload(object payload)
            {
                Envelope.Header.PayloadType = MessagePayloadType.Object;
                Envelope.PayloadObjects = new List<object> { payload };
                return this;
            }

            public MessageEnvelopeBuilder WithCommandStrings(IEnumerable<string> commandStrings)
            {
                Envelope.Header.PayloadType = MessagePayloadType.CommandString;
                if (Envelope.CommandStrings == null)
                    Envelope.CommandStrings = new List<string>();
                Envelope.CommandStrings.AddRange(commandStrings.OrEmptyIfNull());
                return this;
            }

            public MessageEnvelopeBuilder WithCommandString(string commandString)
            {
                Envelope.Header.PayloadType = MessagePayloadType.CommandString;
                if (Envelope.CommandStrings == null)
                    Envelope.CommandStrings = new List<string>();
                Envelope.CommandStrings.Add(commandString);
                return this;
            }

            public MessageEnvelopeBuilder WithRawFrames(IEnumerable<byte[]> frames)
            {
                Envelope.Header.PayloadType = MessagePayloadType.Raw;
                Envelope.RawFrames = frames.OrEmptyIfNull().ToList();
                return this;
            }

            public MessageEnvelopeBuilder ToCluster()
            {
                Envelope.Header.ToType = TargetType.Cluster;
                return this;
            }

            public MessageEnvelopeBuilder ToNode(Guid nodeId)
            {
                Envelope.Header.ToType = TargetType.Node;
                Envelope.Header.ToId = nodeId;
                return this;
            }

            public MessageEnvelopeBuilder ToAppInstance(Guid appInstanceId)
            {
                Envelope.Header.ToType = TargetType.AppInstance;
                Envelope.Header.ToId = appInstanceId;
                return this;
            }

            public MessageEnvelopeBuilder ToZone(string zoneName)
            {
                Envelope.Header.ToType = TargetType.Zone;
                Envelope.Header.ZoneName = zoneName;
                return this;
            }

            public MessageEnvelopeBuilder Local()
            {
                Envelope.Header.ToType = TargetType.Local;
                Envelope.Header.FromType = TargetType.Local;
                return this;
            }

            public MessageEnvelopeBuilder FromNode(Guid nodeId)
            {
                Envelope.Header.FromType = TargetType.Node;
                Envelope.Header.FromId = nodeId;
                return this;
            }

            public MessageEnvelopeBuilder WithEventName(string eventName)
            {
                Envelope.Header.EventName = eventName;
                return this;
            }
        }
    }
}