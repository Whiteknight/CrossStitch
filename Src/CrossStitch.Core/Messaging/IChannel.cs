using System;

namespace CrossStitch.Core.Messaging
{
    public interface IChannel : IDisposable
    {
        void Unsubscribe(Guid id);
    }
}