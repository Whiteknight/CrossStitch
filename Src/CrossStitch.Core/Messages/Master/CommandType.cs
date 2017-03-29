namespace CrossStitch.Core.Messages.Master
{
    public enum CommandType
    {
        Ping,

        UploadPackageFile,
        CreateStitchInstance,

        StartStitchInstance,
        StartStitchGroup,

        StopStitchInstance,
        StopStitchGroup,

        RemoveStitchInstance,
        //CloneStitchInstance
    }
}