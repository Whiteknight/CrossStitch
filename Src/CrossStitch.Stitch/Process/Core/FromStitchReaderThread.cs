using System;
using System.Threading;
using CrossStitch.Stitch.Events;

namespace CrossStitch.Stitch.Process.Core
{
    public class FromStitchReaderThread : IDisposable
    {
        private readonly IMessageChannel _reader;
        private readonly IMessageSerializer _serializer;
        private readonly Thread _readerThread;
        private const int ReaderDelayMs = 1000;

        public FromStitchReaderThread(IMessageChannel reader, IMessageSerializer serializer)
        {
            _reader = reader;
            _serializer = serializer;
            _readerThread = new Thread(ReaderThreadFunction)
            {
                IsBackground = true
            };
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
                var message = Read();
                if (message == null)
                {
                    Thread.Sleep(ReaderDelayMs);
                    continue;
                }
                OnMessageReceived(message);
            }
        }

        private FromStitchMessage Read()
        {
            var messageBuffer = _reader.ReadMessage();
            if (string.IsNullOrEmpty(messageBuffer))
                return null;
            var message = _serializer.DeserializeFromStitchMessage(messageBuffer);
            return message;
        }

        private void OnMessageReceived(FromStitchMessage message)
        {
            MessageReceived.Raise(this, new FromStitchMessageReceivedEventArgs(message));
        }
    }
}