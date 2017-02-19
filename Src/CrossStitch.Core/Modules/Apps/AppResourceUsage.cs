using System;

namespace CrossStitch.Core.Apps
{
    public class AppResourceUsage
    {
        public long UsedMemory { get; set; }
        public long TotalAllocatedMemory { get; set; }
        public TimeSpan ProcessorTime { get; set; }
        public long DiskAppUsageBytes { get; set; }
        public long DiskDataUsageBytes { get; set; }
        public static AppResourceUsage Empty()
        {
            return new AppResourceUsage();
        }
    }
}