namespace CrossStitch.Stitch.v1
{
    public class FromStitchMessage
    {
        public const string CommandAck = "ack";
        public const string CommandFail = "fail";
        public const string CommandSync = "sync";

        public string Command { get; set; }
        public long Id { get; set; }

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

        public static FromStitchMessage Sync()
        {
            return new FromStitchMessage
            {
                Command = CommandSync
            };
        }
    }
}