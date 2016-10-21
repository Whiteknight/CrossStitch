using System;

namespace CrossStitch.Core.Apps
{
    public interface IAppAdaptor : IDisposable
    {
        bool Start();
        void Stop();
        AppResourceUsage GetResources();
        event EventHandler<AppStartedEventArgs> AppInitialized;
    }
}