namespace CrossStitch.Stitch.Process
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
        public string DataChannel { get; set; }
        public string Data { get; set; }
        public string[] Logs { get; set; }
        public string ToGroupName { get; set; }
        public string ToStitchInstanceId { get; set; }

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

        public static FromStitchMessage ToStitchData(string stitchId, string data)
        {
            return new FromStitchMessage
            {
                Command = CommandData,
                Data = data,
                ToStitchInstanceId = stitchId
            };
        }

        public static FromStitchMessage ToGroupData(string groupName, string data)
        {
            return new FromStitchMessage
            {
                Command = CommandData,
                Data = data,
                ToGroupName = groupName
            };
        }

        public static FromStitchMessage Respond(ToStitchMessage message, string data)
        {
            return new FromStitchMessage
            {
                Id = message.Id + 1,
                Command = CommandData,
                Data = data,
                ToStitchInstanceId = message.FromStitchInstanceId
            };
        }

        public bool IsSync()
        {
            return Command == CommandSync;
        }
    }
}