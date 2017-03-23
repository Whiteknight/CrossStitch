using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.ProcessV1;
using CrossStitch.Stitch.ProcessV1.Core;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1
{
    public class ProcessV1StitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        private readonly ProcessV1Parameters _parameters;

        public CoreStitchContext StitchContext { get; }
        private Process _process;
        private CoreMessageManager _channel;

        public ProcessV1StitchAdaptor(StitchInstance stitchInstance, CoreStitchContext stitchContext)
        {
            if (stitchInstance == null)
                throw new ArgumentNullException(nameof(stitchInstance));
            if (stitchContext == null)
                throw new ArgumentNullException(nameof(stitchContext));

            _stitchInstance = stitchInstance;
            StitchContext = stitchContext;
            _parameters = new ProcessV1Parameters(stitchInstance.Adaptor.Parameters);
        }

        public AdaptorType Type => AdaptorType.ProcessV1;

        public bool Start()
        {
            int parentPid = Process.GetCurrentProcess().Id;
            var executableName = Path.Combine(_parameters.DirectoryPath, _parameters.ExecutableName);
            _process = new Process();

            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.ErrorDialog = false;
            _process.StartInfo.FileName = executableName;
            _process.StartInfo.WorkingDirectory = _parameters.DirectoryPath;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            // TODO: What other values should we pass to the new process?
            _process.StartInfo.Arguments = BuildArgumentsString(parentPid);
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

        private string BuildArgumentsString(int parentPid)
        {
            var sb = new StringBuilder();
            AddArgument(sb, Arguments.CorePid, parentPid.ToString());
            AddArgument(sb, Arguments.InstanceId, _stitchInstance.Id);
            AddArgument(sb, Arguments.Application, _stitchInstance.GroupName.Application);
            AddArgument(sb, Arguments.Component, _stitchInstance.GroupName.Component);
            AddArgument(sb, Arguments.Version, _stitchInstance.GroupName.Version);
            AddArgument(sb, Arguments.GroupName, _stitchInstance.GroupName.ToString());
            AddArgument(sb, Arguments.DataDirectory, StitchContext.DataDirectory);
            if (!string.IsNullOrEmpty(_parameters.ExecutableArguments))
            {
                sb.Append("-- ");
                sb.Append(_parameters.ExecutableArguments);
            }
            return sb.ToString();
        }

        private void AddArgument(StringBuilder sb, string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;
            sb.Append(name);
            sb.Append("=");
            sb.Append(value);
            sb.Append(" ");
        }

        public void SendHeartbeat(long id)
        {
            _channel.SendMessage(new ToStitchMessage
            {
                Id = id,
                FromStitchInstanceId = "",
                //NodeId = _nodeContext.NodeId,
                ChannelName = ToStitchMessage.ChannelNameHeartbeat,
                Data = ""
            });
        }

        // TODO: Convert this to take some kind of object instead of all these primitive values
        public void SendMessage(long messageId, string channel, string data, string nodeId, string senderStitchInstanceId)
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