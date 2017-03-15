using System;

namespace CrossStitch.Core.Utility
{
    public interface IModuleLog
    {
        void LogDebug(string fmt, params object[] args);
        void LogError(Exception exception, string fmt, params object[] args);
        void LogError(string fmt, params object[] args);
        void LogInformation(string fmt, params object[] args);
        void LogWarning(string fmt, params object[] args);
    }
}
