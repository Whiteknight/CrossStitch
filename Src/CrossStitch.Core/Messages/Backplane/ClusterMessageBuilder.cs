using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Utility.Extensions;

namespace CrossStitch.Core.Messages.Backplane
{
    public class ClusterMessageBuilder
    {
        private readonly ClusterMessage _envelope;

        public ClusterMessageBuilder()
        {
            _envelope = new ClusterMessage();
        }

        public ClusterMessage Build()
        {
            return _envelope;
        }

        public ClusterMessageBuilder ResponseTo(ClusterMessage request)
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

        public ClusterMessageBuilder FailureResponseTo(ClusterMessage request)
        {
            _envelope.Header.PayloadType = MessagePayloadType.FailureResponse;
            return ResponseTo(request);
        }

        public ClusterMessageBuilder WithObjectPayload(object payloadObject)
        {
            _envelope.Header.PayloadType = MessagePayloadType.Object;
            _envelope.PayloadObject = payloadObject;
            return this;
        }

        public ClusterMessageBuilder WithCommandStrings(IEnumerable<string> commandStrings)
        {
            _envelope.Header.PayloadType = MessagePayloadType.CommandString;
            if (_envelope.CommandStrings == null)
                _envelope.CommandStrings = new List<string>();
            _envelope.CommandStrings.AddRange(commandStrings.OrEmptyIfNull());
            return this;
        }

        public ClusterMessageBuilder WithCommandString(string commandString)
        {
            _envelope.Header.PayloadType = MessagePayloadType.CommandString;
            if (_envelope.CommandStrings == null)
                _envelope.CommandStrings = new List<string>();
            _envelope.CommandStrings.Add(commandString);
            return this;
        }

        public ClusterMessageBuilder WithRawFrames(IEnumerable<byte[]> frames)
        {
            _envelope.Header.PayloadType = MessagePayloadType.Raw;
            _envelope.RawFrames = frames.OrEmptyIfNull().ToList();
            return this;
        }

        public ClusterMessageBuilder ToCluster()
        {
            _envelope.Header.ToType = TargetType.Cluster;
            return this;
        }

        public ClusterMessageBuilder ToNode(string networkNodeId)
        {
            _envelope.Header.ToType = TargetType.Node;
            _envelope.Header.ToNetworkId = networkNodeId;
            return this;
        }

        public ClusterMessageBuilder ToZone(string zoneName)
        {
            _envelope.Header.ToType = TargetType.Zone;
            _envelope.Header.ZoneName = zoneName;
            return this;
        }

        public ClusterMessageBuilder FromNode()
        {
            _envelope.Header.FromType = TargetType.Node;
            return this;
        }

        public ClusterMessageBuilder WithEventName(string eventName)
        {
            _envelope.Header.EventName = eventName;
            return this;
        }
    }
}