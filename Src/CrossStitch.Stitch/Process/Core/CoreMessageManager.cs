using System;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.Process.Core
{
    // Processor class to run on the Core, to coordinate communications with the Stitch. There 
    // should be once instance of this for every stitch.
    public class CoreMessageManager : IDisposable
    {
        private readonly string _instanceId;
        private readonly IStitchEventObserver _observer;
        private readonly IMessageChannel _messageChannel;
        private readonly IMessageSerializer _serializer;
        private FromStitchReaderThread _readerThread;

        public CoreMessageManager(string instanceId, IStitchEventObserver observer, IMessageChannel messageChannel, IMessageSerializer serializer)
        {
            Assert.ArgNotNull(observer, nameof(observer));

            _instanceId = instanceId;
            _observer = observer;
            _messageChannel = messageChannel;
            _serializer = serializer;
        }

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
                    _observer.StitchInstanceManagerOnHeartbeatReceived(_instanceId, message.Id);
                    break;
                case FromStitchMessage.CommandAck:
                    _observer.StitchInstanceManagerOnRequestResponseReceived(_instanceId, message.Id, true);
                    break;
                case FromStitchMessage.CommandFail:
                    _observer.StitchInstanceManagerOnRequestResponseReceived(_instanceId, message.Id, false);
                    break;
                case FromStitchMessage.CommandData:
                    // TODO: This
                    _observer.StitchInstanceManagerOnDataMessageReceived(_instanceId, message.Id, message.ToGroupName, message.ToStitchInstanceId, message.DataChannel, message.Data);
                    break;
                case FromStitchMessage.CommandLogs:
                    _observer.StitchInstanceManagerOnLogsReceived(_instanceId, message.Logs);
                    break;
                default:
                    // TODO: Log that we have received a weird error
                    break;
            }
        }

        public void Dispose()
        {
            _messageChannel?.Dispose();
            _readerThread?.Dispose();
        }
    }
}