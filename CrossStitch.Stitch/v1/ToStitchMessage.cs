namespace CrossStitch.Stitch.v1
{
    public class ToStitchMessage
    {
        // Cluster-unique message Id
        public long Id { get; set; }
        // ID of the Stitch which created this message
        public long StitchId { get; set; }
        // Name of the CrossStich node where this message originated
        public string NodeName { get; set; }
        // The data, as a string.
        public string Data { get; set; }

        public bool IsHeartbeatMessage()
        {
            return false;
        }
    }
}