namespace CrossStitch.Core.Messages.Backplane
{
    public enum MessagePayloadType
    {
        None,
        CommandString,
        Object,
        InternalObject,
        Raw,
        SuccessResponse,
        FailureResponse,
    }
}