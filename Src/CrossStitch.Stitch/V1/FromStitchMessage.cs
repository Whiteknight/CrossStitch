namespace CrossStitch.Stitch.V1
{
    public class FromStitchMessage
    {
        public const string CommandAck = "ack";
        public const string CommandFail = "fail";
        public const string CommandSync = "sync";
        public const string CommandData = "data";
        public const string CommandLogs = "logs";

        public string Command { get; set; }
        public long Id { get; set; }
        public string Data { get; set; }
        public string[] Logs { get; set; }

        public static FromStitchMessage Ack(long id)
        {
            return new FromStitchMessage
            {
                Command = CommandAck,
                Id = id
            };
        }

        public static FromStitchMessage Fail(long id)
        {
            return new FromStitchMessage
            {
                Command = CommandFail,
                Id = id
            };
        }

        public static FromStitchMessage Sync(long heartbeatId)
        {
            return new FromStitchMessage
            {
                Command = CommandSync,
                Id = heartbeatId
            };
        }

        public static FromStitchMessage LogMessage(string[] logs)
        {
            return new FromStitchMessage
            {
                Command = CommandLogs,
                Logs = logs
            };
        }

        public bool IsSync()
        {
            return Command == CommandSync;
        }
    }
}