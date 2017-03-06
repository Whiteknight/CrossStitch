using System;
using System.Threading;
using CrossStitch.Stitch.Events;

namespace CrossStitch.Stitch.V1.Stitch
{
    public class StitchMessageManager : IDisposable
    {
        private readonly IToStitchMessageProcessor _processor;
        private readonly ToStitchMessageReader _reader;
        private readonly FromStitchMessageSender _sender;

        public event EventHandler<HeartbeatReceivedEventArgs> HeartbeatReceived;

        public StitchMessageManager(IToStitchMessageProcessor processor, ToStitchMessageReader reader = null, FromStitchMessageSender sender = null)
        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            _processor = processor;
            _reader = reader ?? new ToStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new FromStitchMessageSender(Console.OpenStandardOutput());
        }

        public void StartRunLoop()
        {
            StartRunLoop(CancellationToken.None);
        }

        public void StartRunLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = _reader.ReadMessage();
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (message == null)
                    continue;

                if (message.IsHeartbeatMessage())
                {
                    OnHeartbeatReceived(message.Id);
                    continue;
                }

                var ok = _processor.Process(message);
                if (ok)
                    _sender.SendAck(message.Id);
                else
                    _sender.SendFail(message.Id);
            }
        }

        private void OnHeartbeatReceived(long id)
        {
            HeartbeatReceived.Raise(this, new HeartbeatReceivedEventArgs
            {
                Id = id
            });
            _sender.SendSync();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _sender.Dispose();
        }
    }
}
