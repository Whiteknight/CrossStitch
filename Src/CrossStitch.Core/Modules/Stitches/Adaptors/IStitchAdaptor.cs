using CrossStitch.Stitch.V1.Core;
using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public interface IStitchAdaptor : IDisposable
    {
        bool Start();
        void Stop();
        void SendHeartbeat(long id);
        void SendMessage(long messageId, string channel, string data, Guid nodeId, long senderId);
        StitchResourceUsage GetResources();
        CoreStitchContext StitchContext { get; }
    }
}