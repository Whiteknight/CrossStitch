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

        public CoreMessageManager(IRunningNodeContext nodeContext, FromStitchMessageReader reader = null, ToStitchMessageSender sender = null)
        {
            _reader = reader ?? new FromStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new ToStitchMessageSender(Console.OpenStandardOutput(), nodeContext);
        }

        // TODO: Redo this all to be async. SendMessage should send and not return a value immediately.
        // _reader should be reading at all times in a tight loop, and putting received messages in
        // an output queue or other storage. When _reader returns a message we parse it and raise one of a
        // number of events

        // TODO: The Stitch should be able to send data (addressed to any other stitch in the same application)
        // or log messages (addressed to the core) without the Core sending a request first. This communication
        // should be fully bi-directional, not request/response.

        public FromStitchMessage SendMessage(ToStitchMessage message, CancellationToken cancellation)
        {
            _sender.SendMessage(message);
            return _reader.ReadMessage(cancellation);
        }

        public FromStitchMessage SendHeartbeat(long id, CancellationToken cancellation)
        {
            _sender.SendHeartbeat(id);
            return _reader.ReadMessage(cancellation);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _sender.Dispose();
        }
    }
}