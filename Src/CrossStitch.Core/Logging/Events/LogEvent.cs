using System;

namespace CrossStitch.Core.Logging.Events
{
    public class LogEvent
    {
        public const string Error = "Error";
        public const string Warning = "Warning";
        public const string Information = "Information";
        public const string Debug = "Debug";

        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
