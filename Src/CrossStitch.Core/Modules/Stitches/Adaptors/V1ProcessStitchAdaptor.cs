using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Events;
using CrossStitch.Stitch.V1.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class V1ProcessStitchAdaptor : IAppAdaptor
    {
        private readonly Instance _instance;
        private Process _process;
        private CoreMessageManager _channel;
        private readonly string _nodeName;

        public event EventHandler<StitchStartedEventArgs> AppInitialized;

        public V1ProcessStitchAdaptor(Instance instance, string nodeName)
        {
            _instance = instance;
            _nodeName = nodeName;
        }

        // TODO: This whole thing needs to be synchronized so we don't attempt to send a new message
        // to the process before the previous message returns a response. We can use a dedicated thread
        // or a queue

        public bool Start()
        {
            var executableName = Path.Combine(_instance.DirectoryPath, _instance.ExecutableName);
            _process = new Process();

            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.ErrorDialog = false;
            _process.StartInfo.FileName = executableName;
            _process.StartInfo.WorkingDirectory = _instance.DirectoryPath;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.Exited += ProcessOnExited;
            _process.Start();

            var fromStitchReader = new FromStitchMessageReader(_process.StandardOutput);
            var toStitchSender = new ToStitchMessageSender(_process.StandardInput, _nodeName);
            _channel = new CoreMessageManager(_nodeName, fromStitchReader, toStitchSender);

            AppInitialized.Raise(this, new StitchStartedEventArgs(_instance.Id));

            return true;
        }

        public void SendMessage(long messageId, string channel, string data, string nodeName, long senderId)
        {
            var response = _channel.SendMessage(new Stitch.V1.ToStitchMessage
            {
                Id = messageId,
                StitchId = senderId,
                NodeName = nodeName,
                ChannelName = channel,
                Data = data
            }, CancellationToken.None);
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            Cleanup(false);
        }

        public void Stop()
        {
            Cleanup(true);
        }

        private void Cleanup(bool requested)
        {
            if (_process != null && !_process.HasExited)
            {
                _process.CancelErrorRead();
                _process.CancelOutputRead();
                _process.Kill();
                _process = null;
            }
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
            if (!requested)
            {
                // Send some kind of event/message up the chain so that the application can know
                // that the process has exited
            }
        }

        public StitchResourceUsage GetResources()
        {
            return new StitchResourceUsage
            {
                ProcessorTime = _process.TotalProcessorTime,
                TotalAllocatedMemory = _process.VirtualMemorySize64,
                UsedMemory = _process.PagedMemorySize64
            };
        }

        public void Dispose()
        {
            Stop();
        }
    }
}