using System;
using System.IO;
using System.Linq;
using System.Security.Policy;
using CrossStitch.App;
using CrossStitch.App.Events;
using CrossStitch.App.Networking;

namespace CrossStitch.Core.Apps
{
    public class AppDomainAppAdaptor : IAppAdaptor
    {
        private readonly ComponentInstance _instance;
        private AppDomain _appDomain;
        private ReceiveChannel _receiver;
        private SendChannel _sender;
        private AppBootloader _bootloader;
        private readonly NetMqMessageMapper _mapper;

        public AppDomainAppAdaptor(ComponentInstance instance)
        {
            _instance = instance;
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        public event EventHandler<AppStartedEventArgs> AppInitialized;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public bool Start()
        {
            _receiver = new ReceiveChannel(_mapper);
            _receiver.MessageReceived += AppMessageReceived;
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
            string assemblyFullName = Path.Combine(_instance.DirectoryPath, _instance.ExecutableName);

            _appDomain = AppDomain.CreateDomain(domainName, evidence, setup);
            var proxyType = typeof(AppBootloader);
            _bootloader = (AppBootloader)_appDomain.CreateInstanceFromAndUnwrap(proxyType.Assembly.Location, proxyType.FullName);
            return _bootloader.StartApp(assemblyFullName, _instance.ApplicationClassName, _receiver.Port);
        }

        public void Stop()
        {
            _bootloader.Stop();
            AppDomain.Unload(_appDomain);
            _receiver.StopListening();
            _receiver.Dispose();
            _receiver = null;
            _sender.Disconnect();
        }

        public bool SendMessage(MessageEnvelope envelope)
        {
            if (_sender == null)
                return false;
            _sender.SendMessage(envelope);
            return true;
        }

        private void AppMessageReceived(object sender, MessageReceivedEventArgs eventArgs)
        {
            var envelope = eventArgs.Envelope;
            if (envelope.Header.ToType == TargetType.Local)
                ReceiveLocalMessage(envelope);
            else
                MessageReceived.Raise(this, eventArgs);
        }

        private void ReceiveLocalMessage(MessageEnvelope envelope)
        {
            if (envelope.Header.PayloadType == MessagePayloadType.CommandString)
            {
                if (envelope.CommandStrings[0] == "App Instance Initialize")
                {
                    var values = envelope.CommandStrings
                        .Skip(1)
                        .Select(s => s.Split(new[] { '=' }, 2))
                        .ToDictionary(a => a[0], a => a[1]);
                    if (_sender != null)
                        _sender.Dispose();
                    _sender = new SendChannel(_mapper);
                    _sender.Connect("tcp://localhost:" + values["ReceivePort"]);
                    AppInitialized.Raise(this, new AppStartedEventArgs(_instance.Id));
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
