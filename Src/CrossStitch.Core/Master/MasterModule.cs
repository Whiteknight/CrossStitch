﻿using Acquaintance;
using CrossStitch.App.Networking;
using CrossStitch.Core.Node;
using System;

namespace CrossStitch.Core.Master
{
    // The master module maintains detailed lists of status and configuration for nodes in the cluster
    // It periodically pings nodes to make sure all information is up to date.
    // It is responsible for sending commands to nodes, and figuring out which commands to send in
    // response to user requests. For example, scheduling and balancing of app instances.
    public class MasterModule : IModule
    {
        // Module responsible for maintaining cluster state
        private RunningNode _runningNode;
        private readonly IClusterNodeManager _nodeManager;

        public MasterModule(IClusterNodeManager nodeManager, IMessageBus messageBus)
        {
            _nodeManager = nodeManager;

            messageBus.Subscribe<MessageEnvelope>(s => s
                .WithChannelName(MessageEnvelope.SendEventName)
                .Invoke(ResolveAppInstanceNodeIdAndSend)
                .OnWorkerThread()
                .WithFilter(IsMessageAddressedToAppInstance)
            );
        }

        private static bool IsMessageAddressedToAppInstance(MessageEnvelope arg)
        {
            return arg.Header.ToType == TargetType.AppInstance;
        }

        private void ResolveAppInstanceNodeIdAndSend(MessageEnvelope obj)
        {
            throw new NotImplementedException();
            // TODO: Resolve the NodeId for the message and publish again.
        }

        public string Name => "Master";

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