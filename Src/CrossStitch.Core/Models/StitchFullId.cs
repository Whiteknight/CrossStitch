using System;

namespace CrossStitch.Core.Models
{
    public class StitchFullId
    {
        public const char FullIdSeparator = ':';

        public StitchFullId(string id)
        {
            var parts = id.Split(FullIdSeparator);
            if (parts.Length == 1)
            {
                NodeId = string.Empty;
                StitchInstanceId = parts[0];
                FullId = id;
            }
            else if (parts.Length == 2)
            {
                NodeId = parts[0];
                StitchInstanceId = parts[1];
                FullId = id;
            }
            else
                throw new Exception("Unknown format for stitch ID");
        }

        public StitchFullId(string nodeId, string id)
        {
            NodeId = nodeId ?? string.Empty;

            // Check if id is already fully-qualified. If so, discard the nodeId portion of it
            var parts = id.Split(FullIdSeparator);
            if (parts.Length == 1)
                StitchInstanceId = parts[0];
            else if (parts.Length == 2)
            {
                StitchInstanceId = parts[1];
                if (string.IsNullOrEmpty(NodeId))
                    NodeId = parts[0];
            }

            if (string.IsNullOrEmpty(NodeId))
                FullId = StitchInstanceId;
            else
                FullId = NodeId + FullIdSeparator + StitchInstanceId;
        }

        public bool IsLocalOnly => string.IsNullOrEmpty(NodeId);
        public string StitchInstanceId { get; }
        public string NodeId { get; }
        public string FullId { get; }
    }
}