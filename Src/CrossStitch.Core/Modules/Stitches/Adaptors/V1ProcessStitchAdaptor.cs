using CrossStitch.Core.Models;
using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Core;
using System;
using System.Diagnostics;
using System.IO;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class V1ProcessStitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        public CoreStitchContext StitchContext { get; }
        private Process _process;
        private CoreMessageManager _channel;

        public V1ProcessStitchAdaptor(StitchInstance stitchInstance, CoreStitchContext stitchContext)
        {
            _stitchInstance = stitchInstance;
            StitchContext = stitchContext;
        }

        public bool Start()
        {
            int parentPid = Process.GetCurrentProcess().Id;
            var executableName = Path.Combine(_stitchInstance.DirectoryPath, _stitchInstance.ExecutableName);
            _process = new Process();

            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.ErrorDialog = false;
            _process.StartInfo.FileName = executableName;
            _process.StartInfo.WorkingDirectory = _stitchInstance.DirectoryPath;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            // TODO: What other values should we pass to the new process?
            _process.StartInfo.Arguments = $"CorePID={parentPid}";
            _process.Exited += ProcessOnExited;

            // TODO: We should pass some command-line args to the new program:
            // node name/id, instance id, some information about the application topology, 
            // the data directory, etc
            bool ok = _process.Start();

            var fromStitchReader = new FromStitchMessageReader(_process.StandardOutput);
            var toStitchSender = new ToStitchMessageSender(_process.StandardInput);
            _channel = new CoreMessageManager(StitchContext, fromStitchReader, toStitchSender);
            _channel.Start();

            StitchContext.RaiseProcessEvent(true, true);

            return true;
        }

        public void SendHeartbeat(long id)
        {
            _channel.SendMessage(new ToStitchMessage
            {
                Id = id,
                FromStitchInstanceId = "",
                //NodeId = _nodeContext.NodeId,
                ChannelName = ToStitchMessage.HeartbeatChannelName,
                Data = ""
            });
        }

        // TODO: Convert this to take some kind of object instead of all these primitive values
        public void SendMessage(long messageId, string channel, string data, Guid nodeId, string senderStitchInstanceId)
        {
            _channel.SendMessage(new ToStitchMessage
            {
                Id = messageId,
                FromStitchInstanceId = senderStitchInstanceId,
                NodeId = nodeId,
                ChannelName = channel,
                Data = data
            });
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            // TODO: Can we get any information about why/how the process exited?
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
            StitchContext.RaiseProcessEvent(false, requested);
        }

        public StitchResourceUsage GetResources()
        {
            return new StitchResourceUsage
            {
                ProcessId = _process.Id,
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