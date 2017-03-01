using System;

namespace CrossStitch.Stitch
{
    public interface IRunningNodeContext
    {
        Guid NodeId { get; set; }
        string Name { get; }
    }
}