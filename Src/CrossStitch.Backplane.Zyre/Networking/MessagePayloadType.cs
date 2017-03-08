namespace CrossStitch.Backplane.Zyre.Networking
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