using Acquaintance.PubSub;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.RequestCoordinator;
using CrossStitch.Core.Utility;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterService
    {
        private readonly CrossStitchCore _core;
        private readonly IModuleLog _log;
        private readonly IDataRepository _data;
        private readonly IStitchRequestHandler _stitchRequests;

        public MasterService(CrossStitchCore core, IModuleLog log, IDataRepository data)
        {
            _core = core;
            _log = log;
            _data = data;
        }

        public NodeStatus GenerateCurrentNodeStatus()
        {
            var message = new NodeStatusBuilder(_core, _data).Build();

            bool ok = _data.Save(message, true);
            if (!ok)
            {
                _log.LogError("Could not save node status");
                return null;
            }
            _log.LogDebug("Published node status");
            return message;
        }

        public NodeStatus GetExistingNodeStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = _core.NodeId;
            return _data.Get<NodeStatus>(id);
        }

        public void SaveNodeStatus(NodeStatus status)
        {
            if (status == null)
                return;
            // TODO: Make sure the values are filled in.
            bool ok = _data.Save(status, true);
            if (!ok)
                _log.LogError("Could not save NodeStatus from NodeId={0}", status.Id);
            else
                _log.LogDebug("Received NodeStatus from NodeId={0} and saved it", status.Id);
        }

        public IEnumerable<IPublishableMessage> EnrichStitchDataMessageWithAddress(StitchDataMessage message)
        {
            var publishable = new List<IPublishableMessage>();
            var messages = new DataMessageAddresser(_data).AddressMessage(message);
            foreach (var outMessage in messages)
            {
                // If it has a Node id, publish it. The filter will stop it from coming back
                // and the Backplane will pick it up.
                if (outMessage.ToNodeId != Guid.Empty)
                {
                    publishable.Add(new PublishableMessage<StitchDataMessage>(null, outMessage));
                    continue;
                }

                // Otherwise, publish it locally for a local stitch instance to grab it.
                publishable.Add(new PublishableMessage<StitchDataMessage>(StitchDataMessage.ChannelSendLocal, outMessage));
            }
            return publishable;
        }

        public CommandResponse DispatchCommandRequest(CommandRequest arg)
        {
            bool ok = false;
            switch (arg.Command)
            {
                case CommandType.StartStitchInstance:
                    // TODO: Lookup the stitch ID. If it is remote, send the command over the backplane
                    // If it is local, do InstanceRequest locally.
                    ok = _stitchRequests.StartInstance(arg.Target);
                    break;
                case CommandType.StopStitchInstance:
                    ok = _stitchRequests.StopInstance(arg.Target);
                    break;
                case CommandType.RemoveStitchInstance:
                    ok = _stitchRequests.RemoveInstance(arg.Target);
                    break;

                default:
                    break;
            }
            return CommandResponse.Create(ok);
        }
    }
}