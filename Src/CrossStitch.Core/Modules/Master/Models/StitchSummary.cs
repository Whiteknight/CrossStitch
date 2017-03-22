using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Master.Models
{
    public class StitchSummary
    {
        public string Id { get; set; }
        public string NodeId { get; set; }
        public string NetworkNodeId { get; set; }
        public StitchGroupName GroupName { get; set; }
    }
}
