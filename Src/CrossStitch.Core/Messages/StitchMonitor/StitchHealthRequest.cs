namespace CrossStitch.Core.Messages.StitchMonitor
{
    public class StitchHealthRequest
    {
        public string StitchId { get; set; }
    }

    public enum StitchHealthType
    {
        Missing,
        Green,
        Yellow,
        Red
    }

    public class StitchHealthResponse
    {
        public StitchHealthType Status { get; set; }
        public string StitchId { get; set; }
        public long LastHeartbeatReceived { get; set; }
        public long HeartbeatsMissed { get; set; }

        public static StitchHealthResponse Create(StitchHealthRequest request, long lastHeartbeatId, long currentHeartbeatId, StitchHealthType type)
        {
            return new StitchHealthResponse
            {
                Status = type,
                StitchId = request.StitchId,
                LastHeartbeatReceived = lastHeartbeatId,
                HeartbeatsMissed = currentHeartbeatId - lastHeartbeatId
            };
        }
    }
}
