using CrossStitch.Core.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public class MessageEnvelopeBuilderFactory
    {
        private readonly Guid _networkNodeId;
        private readonly Guid _nodeId;

        public MessageEnvelopeBuilderFactory(Guid networkNodeId, Guid nodeId)
        {
            _networkNodeId = networkNodeId;
            _nodeId = nodeId;
        }

        public MessageEnvelopeBuilder CreateNew()
        {
            return new MessageEnvelopeBuilder(_networkNodeId, _nodeId);
        }

        public MessageEnvelope CreateEmptyResponse(MessageEnvelope request)
        {
            return CreateNew().ResponseTo(request).Build();
        }

        public MessageEnvelope CreateFailureResponse(MessageEnvelope request)
        {
            return CreateNew().FailureResponseTo(request).Build();
        }
    }

    public class MessageEnvelope
    {
        public const string SendEventName = "Send";
        public const string ReceiveEventName = "Receive";

        public MessageEnvelope()
        {
            Header = new MessageHeader
            {
                PayloadType = MessagePayloadType.None
            };
        }

        public MessageHeader Header { get; set; }
        public List<string> CommandStrings { get; set; }
        public List<object> PayloadObjects { get; set; }
        public List<byte[]> RawFrames { get; set; }
    }

    public class MessageEnvelopeBuilder
    {
        private readonly MessageEnvelope _envelope;
        private readonly Guid _networkNodeId;
        private readonly Guid _nodeId;

        public MessageEnvelopeBuilder(Guid networkNodeId, Guid nodeId)
        {
            _networkNodeId = networkNodeId;
            _nodeId = nodeId;
            _envelope = new MessageEnvelope();
            _envelope.Header.FromNodeId = nodeId;
            _envelope.Header.FromNetworkId = networkNodeId.ToString();
        }

        public MessageEnvelope Build()
        {
            return _envelope;
        }

        public MessageEnvelopeBuilder ResponseTo(MessageEnvelope request)
        {
            _envelope.Header.FromNetworkId = request.Header.ToNetworkId;
            _envelope.Header.FromNodeId = request.Header.ToNodeId;
            _envelope.Header.FromType = request.Header.ToType;
            _envelope.Header.ToNetworkId = request.Header.FromNetworkId;
            _envelope.Header.ToNodeId = request.Header.FromNodeId;
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
            _envelope.PayloadObjects = new List<object>
            {
                payload
            };
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

        public MessageEnvelopeBuilder ToNode(string networkId, Guid nodeId)
        {
            _envelope.Header.ToType = TargetType.Node;
            _envelope.Header.ToNetworkId = networkId;
            _envelope.Header.ToNodeId = nodeId;
            return this;
        }

        public MessageEnvelopeBuilder ToAppInstance(string appInstanceId)
        {
            _envelope.Header.ToType = TargetType.AppInstance;
            _envelope.Header.ToEntityId = appInstanceId;
            return this;
        }

        public MessageEnvelopeBuilder ToZone(string zoneName)
        {
            _envelope.Header.ToType = TargetType.Zone;
            _envelope.Header.ZoneName = zoneName;
            return this;
        }

        public MessageEnvelopeBuilder FromNode()
        {
            _envelope.Header.FromType = TargetType.Node;
            _envelope.Header.FromEntityId = _nodeId.ToString();
            return this;
        }

        public MessageEnvelopeBuilder FromNode(Guid nodeId)
        {
            _envelope.Header.FromType = TargetType.Node;
            _envelope.Header.FromEntityId = nodeId.ToString();
            return this;
        }

        public MessageEnvelopeBuilder WithEventName(string eventName)
        {
            _envelope.Header.EventName = eventName;
            return this;
        }
    }
}