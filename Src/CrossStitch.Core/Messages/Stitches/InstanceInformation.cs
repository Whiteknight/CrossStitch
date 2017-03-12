using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceInformation
    {
        public string Id { get; set; }

        public string GroupName { get; set; }

        public InstanceStateType State { get; set; }

    }
}