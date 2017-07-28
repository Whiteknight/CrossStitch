using System;
using System.Threading;
using CrossStitch.Stitch.Events;

namespace CrossStitch.Stitch.ProcessV1.Core
{
    public class FromStitchReaderThread : IDisposable
    {
        private readonly FromStitchMessageReader _reader;
        private readonly Thread _readerThread;
        private const int ReaderDelayMs = 1000;

        public FromStitchReaderThread(FromStitchMessageReader reader)
        {
            _reader = reader;
            _readerThread = new Thread(ReaderThreadFunction);
            _readerThread.IsBackground = true;
        }

        public event EventHandler<FromStitchMessageReceivedEventArgs> MessageReceived;

        public void Start()
        {
            _readerThread.Start();
        }

        public void Stop()
        {
            if (_readerThread.ThreadState != ThreadState.Running)
                return;
            _readerThread.Abort();
            _readerThread.Join();
        }

        public void Dispose()
        {
            Stop();
        }

        private void ReaderThreadFunction()
        {
            while (true)
            {
                var message = _reader.ReadMessage();
                if (message == null)
                {
                    Thread.Sleep(ReaderDelayMs);
                    continue;
                }
                OnMessageReceived(message);
            }
        }

        private void OnMessageReceived(FromStitchMessage message)
        {
            MessageReceived.Raise(this, new FromStitchMessageReceivedEventArgs(message));
        }
    }
}