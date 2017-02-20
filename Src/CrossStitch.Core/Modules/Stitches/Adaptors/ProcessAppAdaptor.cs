using System;
using System.Diagnostics;
using System.IO;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Events;
using CrossStitch.Core.Utility.Networking;
using NetMQ.Sockets;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class ProcessAppAdaptor : IAppAdaptor
    {
        private readonly Instance _instance;
        private Process _process;

        public event EventHandler<StitchStartedEventArgs> AppInitialized;
        private IReceiveChannel _receiver;
        private RequestSocket _clientSocket;

        public ProcessAppAdaptor(Instance instance, INetwork network)
        {
            _instance = instance;
            _receiver = network.CreateReceiveChannel(false);
        }

        public bool Start()
        {
            _receiver.MessageReceived += MessageReceived;
            _receiver.StartListening("127.0.0.1");

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
            _process.StartInfo.EnvironmentVariables["CS:_communicationPort"] = _receiver.Port.ToString();
            _process.OutputDataReceived += ProcessOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginErrorReadLine();

            _process.Exited += ProcessOnExited;

            AppInitialized.Raise(this, new StitchStartedEventArgs(_instance.Id));

            return true;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            Cleanup(false);
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            // TODO: Write to log
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: Write to log?
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            throw new NotImplementedException();
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
            if (_receiver != null)
            {
                _receiver.StopListening();
                _receiver.Dispose();
                _receiver = null;
            }
            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
                _clientSocket = null;
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