using System;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Core;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessStitchAdaptor : IStitchAdaptor
    {
        private readonly CrossStitchCore _core;
        private readonly StitchInstance _stitchInstance;
        private readonly IStitchEventObserver _observer;
        private readonly ProcessParameters _parameters;
        private readonly IModuleLog _log;
        private readonly CoreMessageChannelFactory _channelFactory;
        private readonly MessageSerializerFactory _serializerFactory;
        private readonly ProcessFactory _processFactory;

        private System.Diagnostics.Process _process;
        private CoreMessageManager _channel;
        private volatile bool _stopRequested;

        public ProcessStitchAdaptor(CrossStitchCore core, StitchesConfiguration configuration, StitchInstance stitchInstance, IStitchEventObserver observer, ProcessParameters parameters, IModuleLog log)
        {
            Assert.ArgNotNull(core, nameof(core));
            Assert.ArgNotNull(configuration, nameof(configuration));
            Assert.ArgNotNull(stitchInstance, nameof(stitchInstance));
            Assert.ArgNotNull(observer, nameof(observer));
            Assert.ArgNotNull(parameters, nameof(parameters));
            Assert.ArgNotNull(log, nameof(log));

            _core = core;
            _stitchInstance = stitchInstance;
            _observer = observer;
            _parameters = parameters;
            _log = log;

            _channelFactory = new CoreMessageChannelFactory(core.NodeId, stitchInstance.Id);
            _serializerFactory = new MessageSerializerFactory();
            _processFactory = new ProcessFactory(stitchInstance, core, log);
            _stopRequested = false;
        }

        public AdaptorType Type => AdaptorType.ProcessV1;

        public bool Start()
        {
            try
            {
                //var data = _stitchInstance.GetAdaptorDataObject<ProcessAdaptorData>() ?? new ProcessAdaptorData();
                // TODO: Need a strategy to handle a zombified child process from the previous attempt
                // Do we try to re-attach? Force-kill it? Alert? Wait?
                //if (data.Pid > 0)
                //    _process = AttachExistingProcess(data.Pid, _parameters.ChannelType);
                //if (_process == null)
                _process = _processFactory.Create(_parameters);
                if (_process == null)
                {
                    _log.LogError("Could not create process");
                    return false;
                }

                _process.Exited += ProcessOnExited;
                PersistProcessData();

                SetupChannel(_parameters.ChannelType, _parameters.SerializerType);

                _observer.StitchStateChanged(_stitchInstance.Id, true, true);

                return true;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not create or start process.");
                _observer.StitchStateChanged(_stitchInstance.Id, false, false);
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
            _stopRequested = true;
            _channel.SendMessage(new ToStitchMessage
            {
                Id = 0,
                NodeId = _core.NodeId,
                ChannelName = ToStitchMessage.ChannelNameExit,
                Data = ""
            });
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
            var messageChannel = _channelFactory.Create(channelType, _process);
            var serializer = _serializerFactory.Create(serializerType);

            _channel = new CoreMessageManager(_stitchInstance.Id, _observer, messageChannel, serializer);
            _channel.Start();
        }

        private void PersistProcessData()
        {
            _stitchInstance.SetAdaptorDataObject(new ProcessAdaptorData
            {
                Pid = _process.Id,
                Name = _process.ProcessName
            });
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            // TODO: Can we get any information about why/how the process exited?
            Cleanup(_stopRequested);
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

            _channel.Dispose();
            _channel = null;

            _observer.StitchStateChanged(_stitchInstance.Id, false, requested);
        }
    }
}