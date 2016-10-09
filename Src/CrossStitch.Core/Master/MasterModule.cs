using System;
using CrossStitch.Core.Backplane;
using CrossStitch.Core.Communications;
using CrossStitch.Core.Messaging;
using CrossStitch.Core.Networking;

namespace CrossStitch.Core.Master
{
    public class MasterModule : IModule
    {
        // Module responsible for maintaining cluster state
        private readonly IClusterBackplane _backplane;
        private RunningNode _runningNode;
        private readonly IClusterNodeManager _nodeManager;
        private readonly IMessageBus _messageBus;

        public MasterModule(IClusterBackplane backplane, IClusterNodeManager nodeManager, IMessageBus messageBus)
        {
            _backplane = backplane;
            _nodeManager = nodeManager;
            _messageBus = messageBus;

            _messageBus.Subscribe<MessageEnvelope>(
                MessageEnvelope.SendEventName,
                ResolveAppInstanceNodeIdAndSend,
                IsMessageAddressedToAppInstance,
                PublishOptions.Default
            );
        }

        private bool IsMessageAddressedToAppInstance(MessageEnvelope arg)
        {
            return arg.Header.ToType == TargetType.AppInstance;
        }

        private void ResolveAppInstanceNodeIdAndSend(MessageEnvelope obj)
        {
            throw new NotImplementedException();
            // TODO: Resolve the NodeId for the message and publish again.
        }

        public string Name { get; private set; }

        public void Start(RunningNode context)
        {
            _runningNode = context;
            _nodeManager.Start();
        }

        public void Stop()
        {
            if (_runningNode == null)
                return;
            _runningNode = null;
            _nodeManager.Stop();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}