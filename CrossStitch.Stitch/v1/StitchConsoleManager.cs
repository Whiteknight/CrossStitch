using System;
using System.Linq;
using System.Threading;

namespace CrossStitch.Stitch.v1
{
    public class StitchConsoleManager : IDisposable
    {
        private readonly IMessageProcessor _processor;
        private readonly StitchMessageReader _reader;
        private readonly StitchMessageSender _sender;

        public StitchConsoleManager(IMessageProcessor processor)
        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            _processor = processor;
            var stdin = Console.OpenStandardInput();
            _reader = new StitchMessageReader(stdin);

            var stdout = Console.OpenStandardOutput();
            _sender = new StitchMessageSender(stdout);
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

                if (message.IsHeartbeatMessage())
                {
                    _sender.SendSync();
                    continue;
                }

                var responses = _processor.Process(message) ?? Enumerable.Empty<FromStitchMessage>();
                foreach (var response in responses)
                {
                    // TODO: Should we force response.Id = message.Id here?
                    _sender.SendMessage(response);
                }
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
            _sender.Dispose();
        }
    }
}
