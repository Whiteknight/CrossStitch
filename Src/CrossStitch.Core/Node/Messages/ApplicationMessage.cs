namespace CrossStitch.Core.Node.Messages
{
    public class ApplicationChangeRequest
    {
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
