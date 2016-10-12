using System;

namespace CrossStitch.Core.Apps
{
    public interface IAppAdaptor : IDisposable
    {
        bool Start();
        void Stop();
        event EventHandler<AppStartedEventArgs> AppInitialized;
    }
}