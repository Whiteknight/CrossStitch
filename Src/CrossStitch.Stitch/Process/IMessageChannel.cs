using System;

namespace CrossStitch.Stitch.Process
{
    public interface IMessageChannel: IDisposable
    {
        string ReadMessage();
        void Send(string message);
    }
}