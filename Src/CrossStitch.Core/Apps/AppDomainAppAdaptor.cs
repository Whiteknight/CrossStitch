using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using CrossStitch.Core.Networking;
using NetMQ.Sockets;

namespace CrossStitch.Core.Apps
{
    public interface IAppAdaptor : IDisposable
    {
        void Start();
        void Stop();
        event EventHandler<AppStartedEventArgs> AppInitialized;
    }

    public class AppStartedEventArgs : EventArgs
    {
        public AppStartedEventArgs(Guid instanceId)
        {
            InstanceId = instanceId;
        }

        public Guid InstanceId { get; private set; }
    }

    public class AppDomainAppAdaptor : IAppAdaptor
    {
        private readonly ComponentInstance _instance;
        private AppDomain _appDomain;
        private ReceiverSocket _receiver;
        private RequestSocket _clientSocket;

        public AppDomainAppAdaptor(ComponentInstance instance)
        {
            _instance = instance;
        }

        public event EventHandler<AppStartedEventArgs> AppInitialized;

        public void Start()
        {
            _receiver = new ReceiverSocket();
            _receiver.MessageReceived += MessageReceived;
            _receiver.StartListening("127.0.0.1");

            string domainName = "Instance_" + _instance.Id.ToString();
            var setup = new AppDomainSetup
            {
                ApplicationName = _instance.FullName,
                ShadowCopyFiles = "true",
                LoaderOptimization = LoaderOptimization.MultiDomainHost,
                ApplicationBase = _instance.DirectoryPath,
                ConfigurationFile = Path.ChangeExtension(_instance.ExecutableName, ".config")
            };

            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);

            _appDomain = AppDomain.CreateDomain(domainName, evidence, setup);
            _appDomain.SetData("CommunicationPort", _receiver.Port);
            // TODO: Load more data in here
            _appDomain.DomainUnload += AppDomainUnload;
            _appDomain.Load(new AssemblyName(_instance.ExecutableName));

            
        }

        public void Stop()
        {
            AppDomain.Unload(_appDomain);
            _receiver.StopListening();
            _receiver.Dispose();
            _receiver = null;
            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
                _clientSocket = null;
            }
        }

        private void AppDomainUnload(object sender, EventArgs e)
        {
            // TODO: Final cleanup here?
        }

        private void OnAppInitialized()
        {
            var handler = AppInitialized;
            if (handler == null)
                return;
            handler(this, new AppStartedEventArgs(_instance.Id));
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
