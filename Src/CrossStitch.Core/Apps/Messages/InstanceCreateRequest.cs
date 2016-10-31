namespace CrossStitch.Core.Apps.Messages
{
    public class InstanceCreateRequest
    {
        public string ApplicationId { get; set; }
        public string ComponentId { get; set; }
        public string VersionId { get; set; }
    }
}
