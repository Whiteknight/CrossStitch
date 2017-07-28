using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Stitch.ProcessV1.Stitch
{
    public class StitchMessageManager : IDisposable
    {
        private const int CoreCheckIntervalMs = 5000;
        private const int ExitBecauseOfCoreDisappearance = 1;
        private const int MessageReadTimeoutMs = 10000;
        private const int ExitCodeCoreMissing = 1;

        private readonly ToStitchMessageReader _reader;
        private readonly FromStitchMessageSender _sender;
        private readonly BlockingCollection<ToStitchMessage> _incomingMessages;

        private Thread _readerThread;
        private Thread _coreMonitorThread;

        public StitchMessageManager(string[] processArgs, ToStitchMessageReader reader = null, FromStitchMessageSender sender = null)
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

            _reader = reader ?? new ToStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new FromStitchMessageSender(Console.OpenStandardOutput());
            _incomingMessages = new BlockingCollection<ToStitchMessage>();
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
                var coreProcess = Process.GetProcessById(corePid);
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
                _sender.SendSync(message.Id);
                return null;
            }

            return message;
        }

        public void Send(FromStitchMessage message)
        {
            _sender.SendMessage(message);
        }

        public void SendLogs(string[] logs)
        {
            _sender.SendMessage(FromStitchMessage.LogMessage(logs));
        }

        public void SyncHeartbeat(long id)
        {
            _sender.SendMessage(FromStitchMessage.Sync(id));
        }

        public void AckMessage(long id)
        {
            _sender.SendMessage(FromStitchMessage.Ack(id));
        }

        public void FailMessage(long id)
        {
            _sender.SendMessage(FromStitchMessage.Fail(id));
        }

        public void Dispose()
        {
            Stop();
            _reader.Dispose();
            _sender.Dispose();
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
                var message = _reader.ReadMessage();
                if (message == null)
                    continue;

                _incomingMessages.Add(message);
            }
        }

        private void CoreCheckerThreadFunction(object coreProcessObject)
        {
            var coreProcess = coreProcessObject as Process;
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
