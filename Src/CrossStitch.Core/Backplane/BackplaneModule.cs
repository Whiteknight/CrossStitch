using System;
using CrossStitch.App.Networking;
using CrossStitch.Core.Messaging;

namespace CrossStitch.Core.Backplane
{
    public sealed class BackplaneModule : IModule
    {
        private readonly BackplaneConfiguration _configuration;
        private readonly IClusterBackplane _backplane;
        private readonly IMessageBus _messageBus;
        private readonly int _workerThreadId;

        public BackplaneModule(BackplaneConfiguration configuration, IClusterBackplane backplane, IMessageBus messageBus)
        {
            _configuration = configuration;
            _backplane = backplane;
            _messageBus = messageBus;

            // Forward messages from the backplane to the IMessageBus
            _backplane.MessageReceived += (s, e) => _messageBus.Publish(e);
            _backplane.ClusterMember += (s, e) => _messageBus.Publish(e);
            _backplane.ZoneMember += (s, e) => _messageBus.Publish(e);

            // Forward messages from the IMessageBus to the backplane
            _workerThreadId = _messageBus.StartDedicatedWorkerThread();
            _messageBus.Subscribe<MessageEnvelope>(
                MessageEnvelope.SendEventName, 
                m => _backplane.Send(m), 
                IsMessageSendable,
                PublishOptions.SpecificThread(_workerThreadId)
            );
        }

        public string Name { get { return "Backplane"; } }

        public void Start(RunningNode context)
        {
            _backplane.Start(context);
        }

        public void Stop()
        {
            _backplane.Stop();
        }

        public void Dispose()
        {
            Stop();
            _messageBus.StopDedicatedWorkerThread(_workerThreadId);
        }

        private bool IsMessageSendable(MessageEnvelope envelope)
        {
            return envelope.Header.ToType == TargetType.Node ||
                   envelope.Header.ToType == TargetType.Zone ||
                   (envelope.Header.ProxyNodeId.HasValue && envelope.Header.ProxyNodeId.Value != Guid.Empty);
        }
    }
}