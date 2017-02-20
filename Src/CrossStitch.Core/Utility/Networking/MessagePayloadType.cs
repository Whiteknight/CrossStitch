namespace CrossStitch.Core.Utility.Networking
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