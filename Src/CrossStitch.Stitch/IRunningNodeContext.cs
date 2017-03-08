using System;

namespace CrossStitch.Stitch
{
    public interface IRunningNodeContext
    {
        Guid NodeId { get; }
        string NetworkNodeId { get; }
    }
}