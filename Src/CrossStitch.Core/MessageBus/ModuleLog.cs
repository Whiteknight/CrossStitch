using Acquaintance;
using CrossStitch.Core.Logging;
using System;

namespace CrossStitch.Core.MessageBus
{
    public class ModuleLog
    {
        private readonly string _moduleName;
        protected IMessageBus MessageBus { get; }

        public ModuleLog(IMessageBus messageBus, string moduleName)
        {
            if (messageBus == null)
                throw new ArgumentNullException(nameof(messageBus));

            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));

            _moduleName = moduleName;
            MessageBus = messageBus;
        }

        protected string GetMessage(string fmt, object[] args)
        {
            return $"[{_moduleName}]: " + string.Format(fmt, args);
        }

        public void LogDebug(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            MessageBus.Publish(LogEvent.LevelDebug, new LogEvent(msg));
        }

        public void LogInformation(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            MessageBus.Publish(LogEvent.LevelInformation, new LogEvent(msg));
        }

        public void LogWarning(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            MessageBus.Publish(LogEvent.LevelWarning, new LogEvent(msg));
        }

        public void LogError(string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            MessageBus.Publish(LogEvent.LevelError, new LogEvent(msg));
        }

        public void LogError(Exception exception, string fmt, params object[] args)
        {
            string msg = GetMessage(fmt, args);
            MessageBus.Publish(LogEvent.LevelError, new LogEvent(msg, exception));
        }
    }
}
