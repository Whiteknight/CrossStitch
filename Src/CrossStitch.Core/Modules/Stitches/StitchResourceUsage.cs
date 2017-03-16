using System;

namespace CrossStitch.Core.Modules.Stitches
{
    // TODO: Move this to the Messages or Models namespace?
    public class StitchResourceUsage
    {
        public int ProcessId { get; set; }
        public long UsedMemory { get; set; }
        public long TotalAllocatedMemory { get; set; }
        public TimeSpan ProcessorTime { get; set; }
        public long DiskAppUsageBytes { get; set; }
        public long DiskDataUsageBytes { get; set; }
        public static StitchResourceUsage Empty()
        {
            return new StitchResourceUsage();
        }
    }
}