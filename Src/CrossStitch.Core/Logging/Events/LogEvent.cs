using System;

namespace CrossStitch.Core.Logging.Events
{
    public class LogEvent
    {
        public const string LevelError = "Error";
        public const string LevelWarning = "Warning";
        public const string LevelInformation = "Information";
        public const string LevelDebug = "Debug";

        public LogEvent(string message, Exception exception = null)
        {
            Message = message;
            Exception = exception;
        }

        public string Message { get; }
        public Exception Exception { get; }
    }
}
