using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{

    public class CreateInstanceRequest
    {
        public const string ChannelCreate = "Create";

        public string Name { get; set; }

        public StitchGroupName GroupName { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

        public bool IsValid()
        {
            return GroupName != null
                && GroupName.IsValid()
                && GroupName.IsVersionGroup()
                && Adaptor != null
                && !string.IsNullOrEmpty(Name);
        }
    }
}