//using CrossStitch.App;
//using CrossStitch.App.Events;
//using CrossStitch.App.Networking;
//using CrossStitch.Core.Data.Entities;
//using System;
//using System.IO;
//using System.Linq;
//using System.Security.Policy;

//namespace CrossStitch.Core.Apps.Adaptors
//{
//    public class AppDomainAppAdaptor : IAppAdaptor
//    {
//        private readonly Instance _instance;
//        private readonly INetwork _network;
//        private AppDomain _appDomain;
//        private readonly IReceiveChannel _receiver;
//        private ISendChannel _sender;
//        private AppBootloader _bootloader;

//        public AppDomainAppAdaptor(Instance instance, INetwork network)
//        {
//            _instance = instance;
//            _network = network;
//            _receiver = network.CreateReceiveChannel(false);
//            _receiver.MessageReceived += AppMessageReceived;
//        }

//        public AppResourceUsage GetResources()
//        {
//            if (_appDomain == null)
//                return AppResourceUsage.Empty();

//            return new AppResourceUsage
//            {
//                UsedMemory = _appDomain.MonitoringSurvivedMemorySize,
//                TotalAllocatedMemory = _appDomain.MonitoringTotalAllocatedMemorySize,
//                ProcessorTime = _appDomain.MonitoringTotalProcessorTime,
//            };
//        }

//        public event EventHandler<AppStartedEventArgs> AppInitialized;
//        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

//        public bool Start()
//        {
//            _receiver.StartListening("127.0.0.1");

//            string domainName = "Instance_" + _instance.Id;
//            var setup = new AppDomainSetup
//            {
//                ApplicationName = _instance.FullName,
//                ShadowCopyFiles = "true",
//                LoaderOptimization = LoaderOptimization.MultiDomainHost,
//                ApplicationBase = _instance.DirectoryPath,
//                ConfigurationFile = Path.ChangeExtension(_instance.ExecutableName, ".config")
//            };

//            var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
//            string assemblyFullName = Path.Combine(_instance.DirectoryPath, _instance.ExecutableName);

//            _appDomain = AppDomain.CreateDomain(domainName, evidence, setup);
//            var proxyType = typeof(AppBootloader);
//            var location = proxyType.Assembly.Location;
//            if (string.IsNullOrEmpty(location))
//                return false;
//            _bootloader = (AppBootloader)_appDomain.CreateInstanceFromAndUnwrap(location, proxyType.FullName);
//            return _bootloader.StartApp(assemblyFullName, _instance.ApplicationClassName, _receiver.Port);
//        }

//        public void Stop()
//        {
//            _bootloader.Stop();
//            // Needed?
//            //RemotingServices.Disconnect(_bootloader);
//            _bootloader = null;

//            AppDomain.Unload(_appDomain);
//            _receiver.StopListening();
//            _sender.Disconnect();
//            _sender.Dispose();
//            _sender = null;
//        }

//        public bool SendMessage(MessageEnvelope envelope)
//        {
//            if (_sender == null)
//                return false;
//            _sender.SendMessage(envelope);
//            return true;
//        }

//        private void AppMessageReceived(object sender, MessageReceivedEventArgs eventArgs)
//        {
//            var envelope = eventArgs.Envelope;
//            if (envelope.Header.ToType == TargetType.Local)
//                ReceiveLocalMessage(envelope);
//            else
//                MessageReceived.Raise(this, eventArgs);
//        }

//        private void ReceiveLocalMessage(MessageEnvelope envelope)
//        {
//            if (envelope.Header.PayloadType == MessagePayloadType.CommandString)
//            {
//                if (envelope.CommandStrings[0] == "App Instance Initialize")
//                {
//                    var values = envelope.CommandStrings
//                        .Skip(1)
//                        .Select(s => s.Split(new[] { '=' }, 2))
//                        .ToDictionary(a => a[0], a => a[1]);
//                    if (_sender != null)
//                        _sender.Dispose();
//                    _sender = _network.CreateSendChannel();
//                    int receivePort = int.Parse(values["ReceivePort"]);
//                    _sender.Connect("localhost", receivePort);
//                    AppInitialized.Raise(this, new AppStartedEventArgs(_instance.Id));
//                }
//            }
//        }

//        public void Dispose()
//        {
//            Stop();
//            _receiver.Dispose();

//        }
//    }
//}
