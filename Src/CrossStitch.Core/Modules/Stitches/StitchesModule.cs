using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Modules.Stitches.Versions;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly StitchesService _service;

        private SubscriptionCollection _subscriptions;

        public StitchesModule(CrossStitchCore core, StitchesConfiguration configuration)
        {
            _messageBus = core.MessageBus;
            var fileSystem = new StitchFileSystem(configuration, new DateTimeVersionManager());
            var manager = new StitchInstanceManager(fileSystem);
            var log = new ModuleLog(_messageBus, Name);
            var observer = new StitchEventObserver(_messageBus, log);
            _service = new StitchesService(fileSystem, manager, observer, log);
        }

        public string Name => ModuleNames.Stitches;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(l => l
                .OnDefaultChannel()
                .Invoke(m => _service.GetInstanceInformation()));
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l
                .WithChannelName(PackageFileUploadRequest.ChannelUpload)
                .Invoke(_service.UploadPackageFile));

            _subscriptions.Listen<EnrichedInstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelCreate)
                .Invoke(_service.CreateNewInstance));
            _subscriptions.Listen<EnrichedInstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStart)
                .Invoke(_service.StartInstance));
            _subscriptions.Listen<EnrichedInstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStop)
                .Invoke(_service.StopInstance));
            _subscriptions.Listen<EnrichedInstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelSendHeartbeat)
                .Invoke(_service.SendHeartbeat));

            _subscriptions.Subscribe<StitchDataMessage>(b => b
                .OnDefaultChannel()
                .Invoke(_service.SendDataMessageToStitch)
                .OnWorkerThread()
                .WithFilter(m => !string.IsNullOrEmpty(m.ToStitchInstanceId)));
        }

        public void Stop()
        {
            _service.StopAll();
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new Dictionary<string, string>
            {
                { "NumberOfStitches", _service.NumberOfRunningStitches.ToString() }
            };
        }

        public void Dispose()
        {
            Stop();
            _service.Dispose();
        }
    }
}
