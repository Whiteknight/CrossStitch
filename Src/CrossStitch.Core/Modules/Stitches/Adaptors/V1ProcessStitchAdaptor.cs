using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Events;
using CrossStitch.Stitch;
using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class V1ProcessStitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        private readonly IRunningNodeContext _nodeContext;
        private Process _process;
        private CoreMessageManager _channel;

        public event EventHandler<StitchProcessEventArgs> StitchInitialized;
        public event EventHandler<StitchProcessEventArgs> StitchExited;

        public V1ProcessStitchAdaptor(StitchInstance stitchInstance, IRunningNodeContext nodeContext)
        {
            _stitchInstance = stitchInstance;
            _nodeContext = nodeContext;
        }

        // TODO: This whole thing needs to be synchronized so we don't attempt to send a new message
        // to the process before the previous message returns a response. We can use a dedicated thread
        // or a queue

        public bool Start()
        {
            var executableName = Path.Combine(_stitchInstance.DirectoryPath, _stitchInstance.ExecutableName);
            _process = new Process();

            var workingDir = Directory.GetCurrentDirectory();

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
            _process.Exited += ProcessOnExited;

            // TODO: We should pass some command-line args to the new program:
            // node name/id, instance id, some information about the application topology, 
            // the data directory, etc
            bool ok = _process.Start();

            var fromStitchReader = new FromStitchMessageReader(_process.StandardOutput);
            var toStitchSender = new ToStitchMessageSender(_process.StandardInput, _nodeContext);
            _channel = new CoreMessageManager(_nodeContext, fromStitchReader, toStitchSender);

            StitchInitialized.Raise(this, new StitchProcessEventArgs(_stitchInstance.Id, true));

            return true;
        }

        public bool SendHeartbeat(long id)
        {
            var response = _channel.SendMessage(new ToStitchMessage
            {
                Id = id,
                StitchId = 0,
                NodeName = _nodeContext.Name,
                ChannelName = ToStitchMessage.HeartbeatChannelName,
                Data = ""
            }, CancellationToken.None);

            return response != null && response.Command == FromStitchMessage.CommandSync;
        }

        // TODO: Convert this to take some kind of object instead of all these primitive values
        // TODO: This should maybe return something more intesting than just a bool. We might want to know
        // how long the round-trip took or other details.
        public bool SendMessage(long messageId, string channel, string data, string nodeName, long senderId)
        {
            var response = _channel.SendMessage(new Stitch.V1.ToStitchMessage
            {
                Id = messageId,
                StitchId = senderId,
                NodeName = nodeName,
                ChannelName = channel,
                Data = data
            }, CancellationToken.None);

            // TODO: Compare IDs? Other verification? Is there other data we want from the Stitch?
            return response.Command == FromStitchMessage.CommandAck;
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
                StitchExited.Raise(this, new StitchProcessEventArgs(_stitchInstance.Id, false));
            }
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