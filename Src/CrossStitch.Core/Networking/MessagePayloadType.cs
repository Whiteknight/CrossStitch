namespace CrossStitch.Core.Networking
{
    public enum MessagePayloadType
    {
        None,
        CommandString,
        Object,
        Raw,
        SuccessResponse,
        FailureResponse,
    }
}