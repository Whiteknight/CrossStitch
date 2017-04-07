namespace CrossStitch.Core.Messages.Security
{
    public enum SecurityRequestType
    {
        Read,
        Create,
        Update,
        Delete
    }

    public class SecurityRequest
    {
        public SecurityRequestType Type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class SecurityResponse
    {
        public bool Allowed { get; set; }
        public string UserName { get; set; }
    }
}
