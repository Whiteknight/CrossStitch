using Acquaintance;
using System;
using CrossStitch.Core.Messages.Logging;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.MessageBus
{
    public class ModuleLog : IModuleLog
    {
        private readonly string _moduleName;
        protected IMessageBus MessageBus { get; }

        public ModuleLog(IMessageBus messageBus, string moduleName)
        {
            Assert.ArgNotNull(messageBus, nameof(messageBus));
            Assert.ArgNotNull(moduleName, nameof(moduleName));

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

        public void LogDebugRaw(string msg)
        {
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
