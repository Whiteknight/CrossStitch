using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public interface IAppAdaptor : IDisposable
    {
        bool Start();
        void Stop();
        StitchResourceUsage GetResources();
        event EventHandler<StitchStartedEventArgs> AppInitialized;
    }
}