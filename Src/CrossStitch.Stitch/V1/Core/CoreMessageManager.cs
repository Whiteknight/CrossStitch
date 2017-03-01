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