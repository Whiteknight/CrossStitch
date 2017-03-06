using Acquaintance;
using System;
using CrossStitch.Core.Logging;

namespace CrossStitch.Core.MessageBus
{
    public class ModuleLog
    {
        private readonly string _moduleName;
        private readonly IMessageBus _messageBus;

        public ModuleLog(IMessageBus messageBus, string moduleName)
        {
            if (messageBus == null)
                throw new ArgumentNullException(nameof(messageBus));

            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));

            _moduleName = moduleName;
            _messageBus = messageBus;
        }

        private string GetMessage(string fmt, object[] args)
        {
            return string.Format("[{0}]", _moduleName) + ": " + string.Format(fmt, args);
        }

        public void LogDebug(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            _messageBus.Publish(LogEvent.LevelDebug, new LogEvent(msg));
        }

        public void LogInformation(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            _messageBus.Publish(LogEvent.LevelInformation, new LogEvent(msg));
        }

        public void LogWarning(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            _messageBus.Publish(LogEvent.LevelWarning, new LogEvent(msg));
        }

        public void LogError(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            _messageBus.Publish(LogEvent.LevelError, new LogEvent(msg));
        }

        public void LogError(Exception exception, string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            _messageBus.Publish(LogEvent.LevelError, new LogEvent(msg, exception));
        }
    }
}
