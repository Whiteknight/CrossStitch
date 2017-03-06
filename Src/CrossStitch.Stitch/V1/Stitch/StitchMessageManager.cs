using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CrossStitch.Stitch.V1.Stitch
{
    public class StitchMessageManager : IDisposable
    {
        private const int CoreCheckIntervalMs = 5000;
        private const int ExitBecauseOfCoreDisappearance = 1;
        private const int MessageReadTimeoutMs = 10000;

        private readonly ToStitchMessageReader _reader;
        private readonly FromStitchMessageSender _sender;
        private readonly int? _corePid;
        private Thread _readerThread;
        private Thread _coreMonitorThread;
        private readonly BlockingCollection<ToStitchMessage> _incomingMessages;

        public bool ReceiveHeartbeats { get; set; }

        public StitchMessageManager(string[] processArgs, ToStitchMessageReader reader = null, FromStitchMessageSender sender = null)
        {
            if (processArgs != null)
            {
                var args = processArgs.Select(s => s.Split('=')).Where(s => s.Length == 2).ToDictionary(s => s[0], s => s[1]);
                if (args.ContainsKey("CorePID"))
                    _corePid = int.Parse(args["CorePID"]);
            }

            _reader = reader ?? new ToStitchMessageReader(Console.OpenStandardInput());
            _sender = sender ?? new FromStitchMessageSender(Console.OpenStandardOutput());
            _incomingMessages = new BlockingCollection<ToStitchMessage>();
        }

        public void Start()
        {
            if (_corePid.HasValue)
            {
                var coreProcess = Process.GetProcessById(_corePid.Value);
                _coreMonitorThread = new Thread(CoreCheckerThreadFunction);
                _coreMonitorThread.Start(coreProcess);
            }

            _readerThread = new Thread(ReaderThreadFunction);
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
            return message;
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

        private void ReaderThreadFunction()
        {
            while (true)
            {
                var message = _reader.ReadMessage();
                if (message == null)
                    continue;

                if (message.IsHeartbeatMessage())
                {
                    if (ReceiveHeartbeats)
                        _incomingMessages.Add(message);
                    else
                        _sender.SendSync(message.Id);

                    continue;
                }

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
                    OnCoreDisappeared();
                    return;
                }
                Thread.Sleep(CoreCheckIntervalMs);
            }
        }

        private void OnCoreDisappeared()
        {
            Environment.Exit(ExitBecauseOfCoreDisappearance);
        }
    }
}
