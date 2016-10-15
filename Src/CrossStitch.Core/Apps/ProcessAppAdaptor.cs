using System;
using System.Diagnostics;
using System.IO;
using CrossStitch.App.Events;
using CrossStitch.App.Networking;
using CrossStitch.Core.Utility.Extensions;
using NetMQ.Sockets;

namespace CrossStitch.Core.Apps
{
    public class ProcessAppAdaptor : IAppAdaptor
    {
        private readonly ComponentInstance _instance;
        private Process _process;
        public event EventHandler<AppStartedEventArgs> AppInitialized;
        private ReceiveChannel _receiver;
        private RequestSocket _clientSocket;

        public ProcessAppAdaptor(ComponentInstance instance)
        {
            _instance = instance;
        }

        public bool Start()
        {
            _receiver = new ReceiveChannel();
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
            _process.StartInfo.EnvironmentVariables["CS:CommunicationPort"] = _receiver.Port.ToString();
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

        public void Dispose()
        {
            Stop();
        }
    }
}