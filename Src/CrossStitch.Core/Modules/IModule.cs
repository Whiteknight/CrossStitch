using System;

namespace CrossStitch.Core.Modules
{
    public static class ModuleNames
    {
        public const string RequestCoordinator = "RequestCoordinator";
        public const string StitchMonitor = "StitchMonitor";
        public const string Stitches = "Stitches";
        public const string Log = "Log";
        public const string Data = "Data";
        public const string Timer = "Timer";
        public const string Backplane = "Backplane";

    }

    public interface IModule : IDisposable
    {
        // A top-level module corresponding to the roles and responsibilities of the local node
        string Name { get; }
        void Start(CrossStitchCore core);
        void Stop();
    }
}