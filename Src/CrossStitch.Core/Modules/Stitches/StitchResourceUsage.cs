using System;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchResourceUsage
    {
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