using CrossStitch.Core.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Backplane.Zyre.Networking
{
    public class MessageEnvelopeBuilderFactory
    {
        private readonly Guid _networkNodeId;
        private readonly string _nodeId;

        public MessageEnvelopeBuilderFactory(Guid networkNodeId, string nodeId)
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
}