namespace CrossStitch.Stitch.V1
{
    public class ToStitchMessage
    {
        public const string HeartbeatChannelName = "_heartbeat";

        // Cluster-unique message Id
        public long Id { get; set; }
        // ID of the Stitch which created this message. 0 if it's coming from CrossStitch core.
        public long StitchId { get; set; }
        // Name of the CrossStich node where this message originated
        public string NodeName { get; set; }
        public string ChannelName { get; set; }

        // The data, as a string.
        // TODO: Decide if this is going to be a json-formatted string or if we are going to use 
        // base64-encoded binary?
        public string Data { get; set; }

        public bool IsHeartbeatMessage()
        {
            return ChannelName == HeartbeatChannelName;
        }
    }
}