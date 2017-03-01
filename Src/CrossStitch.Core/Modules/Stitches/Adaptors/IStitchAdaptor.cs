using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public interface IStitchAdaptor : IDisposable
    {
        bool Start();
        void Stop();
        bool SendHeartbeat(long id);
        bool SendMessage(long messageId, string channel, string data, string nodeName, long senderId);
        StitchResourceUsage GetResources();

        event EventHandler<StitchProcessEventArgs> StitchInitialized;
        event EventHandler<StitchProcessEventArgs> StitchExited;
    }
}