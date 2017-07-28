namespace CrossStitch.Stitch.ProcessV1
{
    public class ToStitchMessage
    {
        public const string ChannelNameHeartbeat = "_heartbeat";
        public const string ChannelNameExit = "_exit";

        // Cluster-unique message Id
        public long Id { get; set; }
        // ID of the Stitch which created this message. 0 if it's coming from CrossStitch core.
        public string FromStitchInstanceId { get; set; }
        // Name of the CrossStich node where this message originated
        public string NodeId { get; set; }
        public string ChannelName { get; set; }

        // The data, as a string. The sender and recipient will decide on the format
        public string Data { get; set; }

        public static ToStitchMessage Exit(int exitCode = 0)
        {
            return new ToStitchMessage
            {
                ChannelName = ChannelNameExit,
                Id = exitCode
            };
        }

        public bool IsHeartbeatMessage()
        {
            return ChannelName == ChannelNameHeartbeat;
        }

        public bool IsExitMessage()
        {
            return ChannelName == ChannelNameExit;
        }
    }
}