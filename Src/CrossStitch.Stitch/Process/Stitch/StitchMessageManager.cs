using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrossStitch.Stitch.Process.Pipes;
using CrossStitch.Stitch.Process.Stdio;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class StitchMessageManager : IDisposable
    {
        private readonly IMessageChannel _messageChannel;
        private readonly IMessageSerializer _serializer;
        private const int CoreCheckIntervalMs = 5000;
        private const int ExitBecauseOfCoreDisappearance = 1;
        private const int MessageReadTimeoutMs = 10000;
        private const int ExitCodeCoreMissing = 1;

        private readonly BlockingCollection<ToStitchMessage> _incomingMessages;

        private Thread _readerThread;
        private Thread _coreMonitorThread;

        public StitchMessageManager(string[] processArgs)
        {
            int i = 0;
            var csArgs = new Dictionary<string, string>();
            for (; i < processArgs.Length; i++)
            {
                string s = processArgs[i];
                if (s == "--")
                {
                    i++;
                    break;
                }

                var parts = s.Split(new[] { '=' }, 2);
                if (parts.Length == 1)
                    csArgs.Add(parts[0], "1");
                else if (parts.Length == 2)
                    csArgs.Add(parts[0], parts[1]);
            }
            CrossStitchArguments = csArgs;
            CustomArguments = processArgs.Skip(i).ToArray();

            _incomingMessages = new BlockingCollection<ToStitchMessage>();

            _messageChannel = GetMessageChannel();

            _serializer = GetSerializer();
        }

        private IMessageSerializer GetSerializer()
        {
            Enum.TryParse(CrossStitchArguments[Arguments.Serializer], out MessageSerializerType serializerType);
            return new JsonMessageSerializer();
        }

        private IMessageChannel GetMessageChannel()
        {
            Enum.TryParse(CrossStitchArguments[Arguments.ChannelType], out MessageChannelType channelType);
            if (channelType == MessageChannelType.Pipe)
            {
                var pipeName = PipeMessageChannel.GetPipeName(CrossStitchArguments[Arguments.CodeId], CrossStitchArguments[Arguments.InstanceId]);
                return new StitchPipeMessageChannel(pipeName);
            }
            return new StdioMessageChannel();
        }

        public bool ReceiveHeartbeats { get; set; }
        public bool ReceiveExitMessage { get; set; }
        public IReadOnlyDictionary<string, string> CrossStitchArguments { get; }
        public string[] CustomArguments { get; }

        public void Start()
        {
            int corePid = GetIntegerArgument(Arguments.CorePid);
            if (corePid > 0)
            {
                var coreProcess = System.Diagnostics.Process.GetProcessById(corePid);
                _coreMonitorThread = new Thread(CoreCheckerThreadFunction);
                _coreMonitorThread.IsBackground = true;
                _coreMonitorThread.Start(coreProcess);
            }

            _readerThread = new Thread(ReaderThreadFunction);
            _readerThread.IsBackground = true;
            _readerThread.Start();
        }

        public void Stop()
        {
            if (_readerThread != null)
            {
                _readerThread.Abort();
                _readerThread.Join();
                _readerThread = null;
            }

            if (_coreMonitorThread != null)
            {
                _coreMonitorThread.Abort();
                _coreMonitorThread.Join();
                _coreMonitorThread = null;
            }
        }

        public ToStitchMessage GetNextMessage()
        {
            ToStitchMessage message;
            bool ok = _incomingMessages.TryTake(out message, MessageReadTimeoutMs);
            if (!ok || message == null)
                return null;

            if (message.IsExitMessage() && !ReceiveExitMessage)
            {
                Environment.Exit((int)message.Id);
            }

            if (message.IsHeartbeatMessage() && !ReceiveHeartbeats)
            {
                SyncHeartbeat(message.Id);
                return null;
            }

            return message;
        }

        public void Send(FromStitchMessage message)
        {
            var messageBuffer = _serializer.Serialize(message);
            _messageChannel.Send(messageBuffer);
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

        public string ApplicationGroupName
        {
            get
            {
                var application = GetCrossStitchArgument(Arguments.Application);
                return application;
            }
        }

        public string ComponentGroupName
        {
            get
            {
                var application = GetCrossStitchArgument(Arguments.Application);
                var component = GetCrossStitchArgument(Arguments.Component);
                return $"{application}.{component}";
            }
        }

        public string VersionGroupName
        {
            get
            {
                var application = GetCrossStitchArgument(Arguments.Application);
                var component = GetCrossStitchArgument(Arguments.Component);
                var version = GetCrossStitchArgument(Arguments.Version);
                return $"{application}.{component}.{version}";
            }
        }

        private int GetIntegerArgument(string name, int defaultValue = 0)
        {
            if (CrossStitchArguments.ContainsKey(name))
                return int.Parse(CrossStitchArguments[name]);
            return defaultValue;
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
                    System.IO.File.AppendAllText("D:\\Test\\StitchStart.Client.txt", e.Message + "\n" + e.StackTrace + "\n");
                }
            }
        }

        private void CoreCheckerThreadFunction(object coreProcessObject)
        {
            var coreProcess = coreProcessObject as System.Diagnostics.Process;
            if (coreProcess == null)
                return;

            while (true)
            {
                if (coreProcess.HasExited)
                {
                    _incomingMessages.Add(ToStitchMessage.Exit(ExitCodeCoreMissing));
                    // After enqueueing the message, we can safely exit this thread. There is no
                    // further need to check anything and a second Exit message will not be
                    // generated.
                    return;
                }
                Thread.Sleep(CoreCheckIntervalMs);
            }
        }

        private string GetCrossStitchArgument(string name)
        {
            return CrossStitchArguments.GetOrDefault(name, string.Empty);
        }
    }
}
