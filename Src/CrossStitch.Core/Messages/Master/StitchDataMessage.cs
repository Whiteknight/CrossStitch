using System;

namespace CrossStitch.Core.Messages.Master
{
    // A data message intended for a Stitch or a collection of Stitches
    public class StitchDataMessage
    {
        public Guid NodeId { get; set; }
        public string NetworkNodeId { get; set; }
        public string Id { get; set; }
        public string Data { get; set; }
    }
}
