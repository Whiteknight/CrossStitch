using System;

namespace CrossStitch.Core.Utility
{
    // IModuleLog implementation which does nothing. Mostly for unit-test scenarios
    public class NullModuleLog : IModuleLog
    {
        public void LogDebug(string fmt, params object[] args)
        {
        }

        public void LogDebugRaw(string msg)
        {
        }

        public void LogError(Exception exception, string fmt, params object[] args)
        {
        }

        public void LogError(string fmt, params object[] args)
        {
        }

        public void LogInformation(string fmt, params object[] args)
        {
        }

        public void LogWarning(string fmt, params object[] args)
        {
        }
    }
}