using System;
using System.Diagnostics;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Core;
using CrossStitch.Stitch.Process.Pipes;
using CrossStitch.Stitch.Process.Stdio;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessStitchAdaptor : IStitchAdaptor
    {
        private readonly string _csNodeId;
        private readonly StitchInstance _stitchInstance;
        private readonly ProcessParameters _parameters;

        public CoreStitchContext StitchContext { get; }
        private System.Diagnostics.Process _process;
        private CoreMessageManager _channel;
        private readonly StitchesConfiguration _configuration;

        public ProcessStitchAdaptor(string csNodeId, StitchesConfiguration configuration, StitchInstance stitchInstance, CoreStitchContext stitchContext, ProcessParameters parameters)
        {
            Assert.ArgNotNull(configuration, nameof(configuration));
            Assert.ArgNotNull(stitchInstance, nameof(stitchInstance));
            Assert.ArgNotNull(stitchContext, nameof(stitchContext));
            Assert.ArgNotNull(parameters, nameof(parameters));

            _csNodeId = csNodeId;
            _stitchInstance = stitchInstance;
            StitchContext = stitchContext;
            _parameters = parameters;
            _configuration = configuration;
        }

        public AdaptorType Type => AdaptorType.ProcessV1;

        public bool Start()
        {
            try
            {
                var data = _stitchInstance.GetAdaptorDataObject<ProcessAdaptorData>() ?? new ProcessAdaptorData();
                if (data.Pid > 0)
                    _process = AttachExistingProcess(data.Pid, _parameters.ChannelType);
                if (_process == null)
                    _process = CreateNewProcess(_parameters.ChannelType);
                ;
                if (_process == null)
                    return false;

                PersistProcessData();

                SetupChannel(_parameters.ChannelType, _parameters.SerializerType);

                StitchContext.RaiseProcessEvent(true, true);

                return true;
            } catch (Exception e)
            {
                return false;
            }
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

        public void Stop()
        {
            _channel.SendMessage(new ToStitchMessage
            {
                Id = 0,
                NodeId = _csNodeId,
                ChannelName = ToStitchMessage.ChannelNameExit,
                Data = ""
            });
            Cleanup(true);
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

        private void SetupChannel(MessageChannelType channelType, MessageSerializerType serializerType)
        {
            var messageChannel = CreateChannel(channelType);
            var serializer = CreateSerializer(serializerType);

            _channel = new CoreMessageManager(StitchContext, messageChannel, serializer);
            _channel.Start();
        }

        private IMessageSerializer CreateSerializer(MessageSerializerType serializerType)
        {
            return new JsonMessageSerializer();
        }

        private IMessageChannel CreateChannel(MessageChannelType channelType)
        {
            if (channelType == MessageChannelType.Pipe)
            {
                var pipeName = PipeMessageChannel.GetPipeName(_csNodeId, _stitchInstance.Id);
                return new CorePipeMessageChannel(pipeName);
            }

            return new StdioMessageChannel(_process.StandardOutput, _process.StandardInput);
            
        }

        private void PersistProcessData()
        {
            _stitchInstance.SetAdaptorDataObject(new ProcessAdaptorData
            {
                Pid = _process.Id,
                Name = _process.ProcessName
            });
        }

        private System.Diagnostics.Process CreateNewProcess(MessageChannelType channelType)
        {
            try
            {
                //var executableName = Path.Combine(_parameters.DirectoryPath, _parameters.ExecutableName);
                _process = CreateNewProcessInternal(channelType == MessageChannelType.Stdio);

                if (_process.Start())
                    return _process;

                _process.Dispose();
                return null;
            }
            catch (Exception e)
            {
                // TODO: Logging
                return null;
            }
        }

        private System.Diagnostics.Process CreateNewProcessInternal(bool useStdio)
        {
            var executableFile = _parameters.ExecutableFormat
                .Replace("{ExecutableName}", _parameters.ExecutableName)
                .Replace("{DirectoryPath}", _parameters.DirectoryPath);

            int parentPid = System.Diagnostics.Process.GetCurrentProcess().Id;

            var coreArgs = new ProcessArguments(StitchContext).BuildCoreArgumentsString(_stitchInstance, _csNodeId, parentPid, _parameters.ChannelType, _parameters.SerializerType);
            var arguments = _parameters.ArgumentsFormat
                .Replace("{ExecutableName}", _parameters.ExecutableName)
                .Replace("{DirectoryPath}", _parameters.DirectoryPath)
                .Replace("{CoreArgs}", coreArgs)
                .Replace("{CustomArgs}", _parameters.ExecutableArguments);

            var process = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = executableFile,
                    WorkingDirectory = _parameters.DirectoryPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = useStdio,
                    RedirectStandardInput = useStdio,
                    RedirectStandardOutput = useStdio,
                    Arguments = arguments
                }
            };

            process.Exited += ProcessOnExited;
            return process;
        }

        private System.Diagnostics.Process AttachExistingProcess(int pid, MessageChannelType channelType)
        {
            var process = System.Diagnostics.Process.GetProcessById(pid);
            _process = process;
            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessOnExited;
            var messageChannel = new StdioMessageChannel(_process.StandardOutput, _process.StandardInput);
            _channel = new CoreMessageManager(StitchContext, messageChannel, new JsonMessageSerializer());
            _channel.Start();
            StitchContext.RaiseProcessEvent(true, true);
            return process;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            // TODO: Can we get any information about why/how the process exited?
            Cleanup(false);
        }

        private void Cleanup(bool requested)
        {
            // TODO: Read and log messages written to STDERR, for cases where, for example, the VM exits with errors before the stitch script is executed
            //var contents = _process.StandardError.ReadToEnd();
            if (_process != null && !_process.HasExited)
            {
                _process.CancelErrorRead();
                _process.CancelOutputRead();
                _process.Kill();
                _process = null;
            }
        
            _channel = null;

            StitchContext.RaiseProcessEvent(false, requested);
        }
    }
}