using CrossStitch.Core.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Backplane.Zyre.Networking
{

    public class MessageEnvelopeBuilder
    {
        private readonly MessageEnvelope _envelope;
        private readonly Guid _networkNodeId;
        private readonly string _nodeId;

        public MessageEnvelopeBuilder(Guid networkNodeId, string nodeId)
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

        public MessageEnvelopeBuilder WithObjectPayload(object payloadObject)
        {
            _envelope.Header.PayloadType = MessagePayloadType.Object;
            _envelope.PayloadObject = payloadObject;
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

        public MessageEnvelopeBuilder ToNode(string networkId, string nodeId)
        {
            _envelope.Header.ToType = TargetType.Node;
            _envelope.Header.ToNetworkId = networkId;
            _envelope.Header.ToNodeId = nodeId;
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