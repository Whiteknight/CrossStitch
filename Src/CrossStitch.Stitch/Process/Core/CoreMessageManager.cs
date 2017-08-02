using System;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.Process.Core
{
    // Processor class to run on the Core, to coordinate communications with the Stitch. There 
    // should be once instance of this for every stitch.
    public class CoreMessageManager 
    {
        private readonly CoreStitchContext _stitchContext;
        private readonly IMessageChannel _messageChannel;
        private readonly IMessageSerializer _serializer;
        private FromStitchReaderThread _readerThread;

        public CoreMessageManager(CoreStitchContext stitchContext, IMessageChannel messageChannel, IMessageSerializer serializer)
        {
            Assert.ArgNotNull(stitchContext, nameof(stitchContext));

            _stitchContext = stitchContext;
            _messageChannel = messageChannel;
            _serializer = serializer;
        }

        public EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public EventHandler<LogsReceivedEventArgs> LogsReceived;

        public void Start()
        {
            _readerThread = new FromStitchReaderThread(_messageChannel, _serializer);
            _readerThread.MessageReceived += ReaderThreadOnMessageReceived;
            _readerThread.Start();
        }

        public void SendMessage(ToStitchMessage message)
        {
            Assert.ArgNotNull(message, nameof(message));
            var messageBuffer = _serializer.Serialize(message);
            _messageChannel.Send(messageBuffer);
        }

        private void ReaderThreadOnMessageReceived(object sender, FromStitchMessageReceivedEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs?.Message?.Command))
                return;

            var message = eventArgs.Message;
            switch (message.Command)
            {
                case FromStitchMessage.CommandSync:
                    _stitchContext.ReceiveHeartbeatSync(message.Id);
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