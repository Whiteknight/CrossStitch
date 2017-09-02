using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class StitchMessageManager : IDisposable
    {
        private const int ExitBecauseOfCoreDisappearance = 1;
        private const int MessageReadTimeoutMs = 10000;

        private readonly IMessageChannel _messageChannel;
        private readonly IMessageSerializer _serializer;
        private readonly System.Diagnostics.Process _parentProcess;
        private readonly BlockingCollection<ToStitchMessage> _incomingMessages;
        private readonly Thread _readerThread;

        public StitchMessageManager(string[] processArgs)
        {
            var args = new StitchArgumentParser().Parse(processArgs);
            CustomArguments = args.CustomArguments;
            CrossStitchParameters = args.GetCoreArgumentsObject();

            int corePid = CrossStitchParameters.CorePid;
            if (corePid > 0)
                _parentProcess = System.Diagnostics.Process.GetProcessById(corePid);

            _incomingMessages = new BlockingCollection<ToStitchMessage>();

            _messageChannel = new StitchMessageChannelFactory(CrossStitchParameters.CoreId, CrossStitchParameters.InstanceId).Create(CrossStitchParameters.MessageChannelType);

            _serializer = new MessageSerializerFactory().Create(CrossStitchParameters.MessageSerializerType);

            _readerThread = new Thread(ReaderThreadFunction)
            {
                IsBackground = true
            };
        }

        public bool ReceiveHeartbeats { get; set; }
        public bool ReceiveExitMessage { get; set; }
        public bool ReceiveErrorMessages { get; set; }
        public CrossStitchArguments CrossStitchParameters { get; }
        public IReadOnlyList<string> CustomArguments { get; }

        public void Start()
        {
            _readerThread.Start();
        }

        public void Stop()
        {
            _readerThread?.Abort();
            _readerThread?.Join();
        }

        public ToStitchMessage GetNextMessage()
        {
            while (true)
            {
                var message = GetNextMessageInternal();
                if (message == null)
                    return null;

                // Exit message. If the app wants them, return it. Otherwise we exit.
                if (message.IsExitMessage())
                {
                    if (ReceiveExitMessage)
                        return message;
                    Stop();
                    Environment.Exit((int) message.Id);
                }

                // Heartbeat message. If the app wants them, return it. Otherwise sync immediately.
                if (message.IsHeartbeatMessage())
                {
                    if (ReceiveHeartbeats)
                        return message;
                    SyncHeartbeat(message.Id);
                    continue;
                }

                // Error message from the StitchMessageManager machinery. If the app wants them, return it.
                // Otherwise we ignore it.
                if (message.IsErrorMessage())
                {
                    if (ReceiveErrorMessages)
                        return message;
                    continue;
                }

                return message;
            }
        }

        private ToStitchMessage GetNextMessageInternal()
        {
            bool ok = _incomingMessages.TryTake(out ToStitchMessage message, MessageReadTimeoutMs);
            if (ok && message != null)
                return message;

            // No messages. Check the health of the core and return an exist message if it doesn't exist
            if (_parentProcess != null && _parentProcess.HasExited)
                return ToStitchMessage.Exit(ExitBecauseOfCoreDisappearance);

            // No messages. Return null;
            return null;
        }

        public void Send(FromStitchMessage message)
        {
            var messageBuffer = _serializer.Serialize(message);
            _messageChannel.Send(messageBuffer);
        }

        public void SendLogs(string logs)
        {
            Send(FromStitchMessage.LogMessage(new [] { logs }));
        }

        public void SendLogs(string[] logs)
        {
            Send(FromStitchMessage.LogMessage(logs));
        }

        public void SyncHeartbeat(long id)
        {
            Send(FromStitchMessage.Sync(id));
        }

        public void AckMessage(long id)
        {
            Send(FromStitchMessage.Ack(id));
        }

        public void FailMessage(long id)
        {
            Send(FromStitchMessage.Fail(id));
        }

        public void Dispose()
        {
            Stop();
        }

        private void ReaderThreadFunction()
        {
            while (true)
            {
                try
                {
                    var messageBuffer = _messageChannel.ReadMessage();
                    if (string.IsNullOrEmpty(messageBuffer))
                        continue;
                    var message = _serializer.DeserializeToStitchMessage(messageBuffer);
                    _incomingMessages.Add(message);
                }
                catch (Exception e)
                {
                    _incomingMessages.Add(ToStitchMessage.Error(e));
                }
            }
        }
    }
}
