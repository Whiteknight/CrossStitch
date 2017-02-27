using System;
using System.Threading;

namespace CrossStitch.Stitch.V1.Core
{
    // Processor class to run on the Core, to coordinate communications with the Stitch. There 
    // should be once instance of this for every stitch.
    public class CoreMessageManager : IDisposable
    {
        private readonly FromStitchMessageReader _reader;
        private readonly ToStitchMessageSender _sender;

        public CoreMessageManager(string nodeName, FromStitchMessageReader reader = null, ToStitchMessageSender sender = null)
        {
            _reader = reader ?? new FromStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new ToStitchMessageSender(Console.OpenStandardOutput(), nodeName);
        }

        public void StartRunLoop()
        {
            StartRunLoop(CancellationToken.None);
        }

        public void StartRunLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = _reader.ReadMessage(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (message == null)
                    continue;

                switch (message.Command)
                {
                    case FromStitchMessage.CommandSync:
                        OnSyncReceived(message);
                        break;
                    case FromStitchMessage.CommandAck:
                        OnAckReceived(message);
                        break;
                    case FromStitchMessage.CommandFail:
                        OnFailReceived(message);
                        break;
                }
            }
        }

        // TODO: Setup events for all of these cases.

        private void OnFailReceived(FromStitchMessage message)
        {
            throw new NotImplementedException();
        }

        private void OnAckReceived(FromStitchMessage message)
        {
            throw new NotImplementedException();
        }

        private void OnSyncReceived(FromStitchMessage message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _sender.Dispose();
        }
    }
}