namespace CrossStitch.Core.Modules.Stitches.Messages
{
    public class InstanceRequest
    {
        public const string Start = "Start";
        public const string Stop = "Stop";
        public const string Clone = "Clone";
        public const string Delete = "Delete";

        public string Id { get; set; }
    }

    public class InstanceResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }

        public static InstanceResponse Failure()
        {
            return new InstanceResponse
            {
                Success = false
            };
        }
    }
}
