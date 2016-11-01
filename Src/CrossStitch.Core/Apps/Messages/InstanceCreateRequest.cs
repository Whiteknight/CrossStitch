namespace CrossStitch.Core.Apps.Messages
{
    public class InstanceCreateRequest
    {
        public string Application { get; set; }
        public string Component { get; set; }
        public string Version { get; set; }
    }
}
