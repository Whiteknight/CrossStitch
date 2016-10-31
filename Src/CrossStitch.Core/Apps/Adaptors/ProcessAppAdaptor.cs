using CrossStitch.App.Events;
using CrossStitch.App.Networking;
using CrossStitch.Core.Data.Entities;
using NetMQ.Sockets;
using System;
using System.Diagnostics;
using System.IO;

namespace CrossStitch.Core.Apps
{
    public class ProcessAppAdaptor : IAppAdaptor
    {
        private readonly Instance _instance;
        private readonly INetwork _network;
        private Process _process;
        

        public event EventHandler<AppStartedEventArgs> AppInitialized;
        private IReceiveChannel _receiver;
        private RequestSocket _clientSocket;

        public ProcessAppAdaptor(Instance instance, INetwork network)
        {
            _instance = instance;
            _network = network;
            _receiver = _network.CreateReceiveChannel(false);
        }

        public bool Start()
        {
            _receiver.MessageReceived += MessageReceived;
            _receiver.StartListening("127.0.0.1");

            var executableName = Path.Combine(_instance.DirectoryPath, _instance.ExecutableName);
            _process = new Process();

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

            AppInitialized.Raise(this, new AppStartedEventArgs(_instance.Id));

            return true;
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
            _process.CancelErrorRead();
            _process.CancelOutputRead();
            _process.Kill();
            _process = null;

            _receiver.StopListening();
            _receiver.Dispose();
            _receiver = null;
            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
                _clientSocket = null;
            }
        }

        public AppResourceUsage GetResources()
        {
            return new AppResourceUsage {
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