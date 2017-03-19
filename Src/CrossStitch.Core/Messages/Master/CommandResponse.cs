namespace CrossStitch.Core.Messages.Master
{
    public class CommandResponse
    {
        public CommandResultType Result { get; set; }
        public string ScheduledJobId { get; set; }

        public static CommandResponse Create(bool ok)
        {
            return new CommandResponse
            {
                Result = ok ? CommandResultType.Success : CommandResultType.Error
            };
        }

        public static CommandResponse Started(string jobId)
        {
            return new CommandResponse
            {
                Result = CommandResultType.Started,
                ScheduledJobId = jobId
            };
        }

    }
}