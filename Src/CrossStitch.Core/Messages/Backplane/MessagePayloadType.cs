namespace CrossStitch.Core.Messages.Backplane
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