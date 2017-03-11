using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceInformation
    {
        public string Id { get; set; }

        public string FullVersionName { get; set; }

        public InstanceStateType State { get; set; }

    }
}