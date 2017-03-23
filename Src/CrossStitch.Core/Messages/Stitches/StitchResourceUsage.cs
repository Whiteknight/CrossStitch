using System;

namespace CrossStitch.Core.Messages.Stitches
{
    public class StitchResourceUsageRequest
    {
        public string StitchInstanceId { get; set; }
    }

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