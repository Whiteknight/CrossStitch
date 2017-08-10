﻿using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Versions;
using System.Collections.Generic;
using CrossStitch.Core.Modules.Stitches.Adaptors;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesModule : IModule
    {
        private readonly StitchesService _service;
        private readonly SubscriptionCollection _subscriptions;

        public StitchesModule(CrossStitchCore core, StitchesConfiguration configuration = null)
        {
            configuration = configuration ?? StitchesConfiguration.GetDefault();
            var log = new ModuleLog(core.MessageBus, Name);
            var fileSystem = new StitchFileSystem(configuration, new DateTimeVersionManager());
            var adaptorFactory = new StitchAdaptorFactory(core, configuration, fileSystem, log);
            var manager = new StitchInstanceManager(fileSystem, adaptorFactory);
            var observer = new StitchEventObserver(core.MessageBus, log);
            var data = new DataHelperClient(core.MessageBus);
            var notifier = new StitchEventNotifier(core.MessageBus);
            _service = new StitchesService(core, data, fileSystem, manager, observer, log, notifier);
            _subscriptions = new SubscriptionCollection(core.MessageBus);
        }

        public string Name => ModuleNames.Stitches;

        public void Start()
        {
            // On Core initialization, startup all necessary Stitches
            _subscriptions.Subscribe<CoreEvent>(b => b
                .WithChannelName(CoreEvent.ChannelInitialized)
                .Invoke(m => _service.StartRunningStitchesOnStartup())
                .OnWorkerThread());

            // Upload package files
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l
                .WithChannelName(PackageFileUploadRequest.ChannelLocal)
                .Invoke(_service.UploadStitchPackageFile));
            _subscriptions.Listen<PackageFileUploadRequest, PackageFileUploadResponse>(l => l
                .WithChannelName(PackageFileUploadRequest.ChannelFromRemote)
                .Invoke(_service.UploadStitchPackageFileFromRemote));

            _subscriptions.Listen<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(l => l
                .OnDefaultChannel()
                .Invoke(_service.CreateNewInstance));

            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelClone)
                .Invoke(_service.CloneInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStart)
                .Invoke(_service.StartInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelStop)
                .Invoke(_service.StopInstance));
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(l => l
                .WithChannelName(InstanceRequest.ChannelDelete)
                .Invoke(_service.DeleteStitchInstance));

            _subscriptions.Listen<InstanceInformationRequest, List<InstanceInformation>>(l => l
                .OnDefaultChannel()
                .Invoke(m => _service.GetInstanceInformation()));
            _subscriptions.Listen<StitchResourceUsageRequest, StitchResourceUsage>(l => l
                .OnDefaultChannel()
                .Invoke(m => _service.GetInstanceResources(m.StitchInstanceId)));

            _subscriptions.Subscribe<StitchDataMessage>(b => b
                .WithChannelName(StitchDataMessage.ChannelSendLocal)
                .Invoke(_service.SendDataMessageToStitch)
                .OnWorkerThread()
                .WithFilter(m => !string.IsNullOrEmpty(m.ToStitchInstanceId)));
            _subscriptions.Subscribe<ObjectReceivedEvent<StitchDataMessage>>(b => b
                .WithChannelName(ReceivedEvent.ChannelReceived)
                .Invoke(m => _service.SendDataMessageToStitch(m.Object)));

            _subscriptions.Subscribe<SendHeartbeatEvent>(b => b
                .OnDefaultChannel()
                .Invoke(m => _service.SendHeartbeat(m.HeartbeatId)));
        }

        public void Stop()
        {
            _service.StopAllOnShutdown();
            _subscriptions.Dispose();
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

        private class StitchEventNotifier : IStitchEventNotifier
        {
            private readonly IMessageBus _messageBus;

            public StitchEventNotifier(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void StitchCreated(StitchInstance instance)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelCreated, new StitchInstanceEvent
                {
                    InstanceId = instance.Id,
                    GroupName = instance.GroupName
                });
            }

            public void StitchStarted(StitchInstance instance)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelStarted, new StitchInstanceEvent
                {
                    InstanceId = instance.Id,
                    GroupName = instance.GroupName
                });
            }

            public void StitchStopped(StitchInstance instance)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelStopped, new StitchInstanceEvent
                {
                    InstanceId = instance.Id,
                    GroupName = instance.GroupName
                });
            }

            public void StitchDeleted(string id, StitchGroupName groupName)
            {
                _messageBus.Publish(StitchInstanceEvent.ChannelCreated, new StitchInstanceEvent
                {
                    InstanceId = id,
                    GroupName = groupName
                });
            }
        }
    }
}
