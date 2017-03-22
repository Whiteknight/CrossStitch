namespace CrossStitch.Core.Messages.Master
{
    public enum CommandType
    {
        Ping,

        StartStitchInstance,
        StartStitchGroup,

        StopStitchInstance,
        StopStitchGroup,

        RemoveStitchInstance,
        //CloneStitchInstance
    }
}