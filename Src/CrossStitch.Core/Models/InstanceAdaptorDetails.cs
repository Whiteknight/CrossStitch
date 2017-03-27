using System.Collections.Generic;

namespace CrossStitch.Core.Models
{
    public class InstanceAdaptorDetails
    {
        public AdaptorType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}