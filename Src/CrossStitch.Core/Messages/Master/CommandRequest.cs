namespace CrossStitch.Core.Messages.Master
{
    public enum CommandType
    {
        StartStitchInstance,
        StopStitchInstance,
        RemoveStitchInstance,
        CloneStitchInstance
    }

    public class CommandRequest
    {
        public CommandType Command { get; set; }
        public string Target { get; set; }
    }

    public enum CommandResultType
    {
        Success,
        Error,
        Scheduled
    }

    public class CommandResponse
    {
        public static CommandResponse Create(bool ok)
        {
            return new CommandResponse
            {
                Result = ok ? CommandResultType.Success : CommandResultType.Error
            };
        }
        public CommandResultType Result { get; set; }
        public string ScheduledJobId { get; set; }
    }
}
