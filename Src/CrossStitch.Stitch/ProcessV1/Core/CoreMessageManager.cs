using System;

namespace CrossStitch.Stitch.V1.Core
{
    // Processor class to run on the Core, to coordinate communications with the Stitch. There 
    // should be once instance of this for every stitch.
    public class CoreMessageManager : IDisposable
    {
        private readonly CoreStitchContext _stitchContext;
        private readonly FromStitchMessageReader _reader;
        private readonly ToStitchMessageSender _sender;
        private FromStitchReaderThread _readerThread;

        public CoreMessageManager(CoreStitchContext stitchContext, FromStitchMessageReader reader = null, ToStitchMessageSender sender = null)
        {
            if (stitchContext == null)
                throw new ArgumentNullException(nameof(stitchContext));

            _stitchContext = stitchContext;
            _reader = reader ?? new FromStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new ToStitchMessageSender(Console.OpenStandardOutput());
        }

        public EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public EventHandler<LogsReceivedEventArgs> LogsReceived;

        public void Start()
        {
            _readerThread = new FromStitchReaderThread(_reader);
            _readerThread.MessageReceived += ReaderThreadOnMessageReceived;
            _readerThread.Start();
        }

        public void SendMessage(ToStitchMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            _sender.SendMessage(message);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _sender.Dispose();
        }

        private void ReaderThreadOnMessageReceived(object sender, FromStitchMessageReceivedEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs?.Message?.Command))
                return;

            var message = eventArgs.Message;
            switch (message.Command)
            {
                case FromStitchMessage.CommandSync:
                    _stitchContext.ReceiveHeartbeat(message.Id);
                    break;
                case FromStitchMessage.CommandAck:
                    _stitchContext.ReceiveResponse(message.Id, true);
                    break;
                case FromStitchMessage.CommandFail:
                    _stitchContext.ReceiveResponse(message.Id, false);
                    break;
                case FromStitchMessage.CommandData:
                    // TODO: This
                    _stitchContext.ReceiveData(message.Id, message.ToGroupName, message.ToStitchInstanceId, message.DataChannel, message.Data);
                    break;
                case FromStitchMessage.CommandLogs:
                    _stitchContext.ReceiveLogs(message.Logs);
                    break;
                default:
                    // TODO: Log that we have received a weird error
                    break;
            }
        }
    }
}