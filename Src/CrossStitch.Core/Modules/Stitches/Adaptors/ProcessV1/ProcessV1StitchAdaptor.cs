using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.ProcessV1;
using CrossStitch.Stitch.ProcessV1.Core;
using System;
using System.Diagnostics;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1
{
    public class ProcessV1StitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        private readonly ProcessV1Parameters _parameters;

        public CoreStitchContext StitchContext { get; }
        private Process _process;
        private CoreMessageManager _channel;
        private readonly StitchesConfiguration _configuration;

        public ProcessV1StitchAdaptor(StitchesConfiguration configuration, StitchInstance stitchInstance, CoreStitchContext stitchContext, ProcessV1Parameters parameters)
        {
            Assert.ArgNotNull(configuration, nameof(configuration));
            Assert.ArgNotNull(stitchInstance, nameof(stitchInstance));
            Assert.ArgNotNull(stitchContext, nameof(stitchContext));
            Assert.ArgNotNull(parameters, nameof(parameters));

            _stitchInstance = stitchInstance;
            StitchContext = stitchContext;
            _parameters = parameters;
            _configuration = configuration;
        }

        public AdaptorType Type => AdaptorType.ProcessV1;

        public bool Start()
        {
            var data = _stitchInstance.GetAdaptorDataObject<ProcessV1AdaptorData>();
            if (data != null && data.Pid > 0)
                _process = AttachExistingProcess(data.Pid);
            if (_process == null)
                _process = CreateNewProcess(); ;
            if (_process == null)
                return false;

            PersistProcessData();

            SetupChannel();

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

        private void SetupChannel()
        {
            var fromStitchReader = new FromStitchMessageReader(_process.StandardOutput);
            var toStitchSender = new ToStitchMessageSender(_process.StandardInput);
            _channel = new CoreMessageManager(StitchContext, fromStitchReader, toStitchSender);
            _channel.Start();
        }

        private void PersistProcessData()
        {
            _stitchInstance.SetAdaptorDataObject(new ProcessV1AdaptorData
            {
                Pid = _process.Id,
                Name = _process.ProcessName
            });
        }

        private Process CreateNewProcess()
        {
            try
            {
                //var executableName = Path.Combine(_parameters.DirectoryPath, _parameters.ExecutableName);
                var executableFile = _parameters.ExecutableFormat
                    .Replace("{ExecutableName}", _parameters.ExecutableName)
                    .Replace("{DirectoryPath}", _parameters.DirectoryPath);

                int parentPid = Process.GetCurrentProcess().Id;

                var coreArgs = new ProcessV1ArgsBuilder(StitchContext).BuildCoreArgumentsString(_stitchInstance, parentPid);
                var arguments = _parameters.ArgumentsFormat
                    .Replace("{ExecutableName}", _parameters.ExecutableName)
                    .Replace("{DirectoryPath}", _parameters.DirectoryPath)
                    .Replace("{CoreArgs}", coreArgs)
                    .Replace("{CustomArgs}", _parameters.ExecutableArguments);

                var process = new Process
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
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        Arguments = arguments
                    }
                };

                process.Exited += ProcessOnExited;

                if (_process.Start())
                    return process;

                process.Dispose();
                return null;
            }
            catch (Exception e)
            {
                // TODO: Logging
                return null;
            }
        }

        private Process AttachExistingProcess(int pid)
        {
            var process = Process.GetProcessById(pid);
            _process = process;
            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessOnExited;
            var fromStitchReader = new FromStitchMessageReader(_process.StandardOutput);
            var toStitchSender = new ToStitchMessageSender(_process.StandardInput);
            _channel = new CoreMessageManager(StitchContext, fromStitchReader, toStitchSender);
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
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
            StitchContext.RaiseProcessEvent(false, requested);
        }
    }
}