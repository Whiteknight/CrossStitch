using Acquaintance;
using CrossStitch.Core.Logging.Events;
using System;

namespace CrossStitch.Core.MessageBus
{
    public static class MessageBusExtensions
    {
        public static void LogDebug(this IPublishable messageBus, string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            messageBus.Publish(LogEvent.LevelDebug, new LogEvent(msg));
        }

        public static void LogInformation(this IPublishable messageBus, string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            messageBus.Publish(LogEvent.LevelInformation, new LogEvent(msg));
        }

        public static void LogWarning(this IPublishable messageBus, string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            messageBus.Publish(LogEvent.LevelWarning, new LogEvent(msg));
        }

        public static void LogError(this IPublishable messageBus, string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            messageBus.Publish(LogEvent.LevelError, new LogEvent(msg));
        }

        public static void LogError(this IPublishable messageBus, Exception exception, string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            messageBus.Publish(LogEvent.LevelError, new LogEvent(msg, exception));
        }
    }
}
