using Acquaintance;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Handlers;
using CrossStitch.Core.Utility;
using CrossStitch.Core.Utility.Extensions;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterService
    {
        private readonly CrossStitchCore _core;
        private readonly IModuleLog _log;
        private readonly MasterDataRepository _data;
        private readonly IStitchRequestHandler _stitches;
        private readonly Dictionary<CommandType, ICommandHandler> _commandHandlers;
        private readonly IClusterMessageSender _clusterSender;

        public MasterService(CrossStitchCore core, IModuleLog log, MasterDataRepository data, IStitchRequestHandler stitches)
        {
            _core = core;
            _log = log;
            _data = data;
            _stitches = stitches;
            _clusterSender = new ClusterMessageSender(core.MessageBus);

            _commandHandlers = new Dictionary<CommandType, ICommandHandler>
            {
                { CommandType.Ping, new PingCommandHandler(data, _clusterSender) },
                { CommandType.StartStitchInstance, new StartStitchCommandHandler(data, stitches, _clusterSender) },
                { CommandType.StopStitchInstance, new StopStitchCommandHandler(data, stitches, _clusterSender) },
                { CommandType.RemoveStitchInstance, new RemoveStitchCommandHandler(data, stitches, _clusterSender) },
                { CommandType.StartStitchGroup, new StartAllStitchGroupCommandHandler(core.NodeId, data, stitches, _clusterSender) },
                { CommandType.StopStitchGroup, new StopAllStitchGroupCommandHandler(core.NodeId, data, stitches, _clusterSender) }
            };
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

        public void EnrichStitchDataMessageWithAddress(StitchDataMessage message)
        {
            var messages = new DataMessageAddresser(_core.NodeId, _data).AddressMessage(message);
            foreach (var outMessage in messages)
            {
                bool isRemote = !string.IsNullOrEmpty(outMessage.ToNodeId);
                // If it has a Node id, publish it. The filter will stop it from coming back
                // and the Backplane will pick it up.
                // Otherwise, publish it locally for a local stitch instance to grab it.
                _stitches.SendStitchData(outMessage, isRemote);
                if (isRemote)
                    _log.LogDebug("Routing StitchDataMessage to node Id={0}, StitchId={1}", outMessage.ToNodeId, outMessage.ToStitchInstanceId);
            }
        }

        public void ReceiveCommandFromRemote(ReceivedEvent received, CommandRequest request)
        {
            var handler = _commandHandlers.GetOrDefault(request.Command);
            // TODO: Alert back that we can't handle this case?
            if (handler == null)
            {
                if (request.RequestsReceipt())
                    _clusterSender.SendReceipt(false, received.FromNetworkId, request.ReplyToJobId, request.ReplyToTaskId);
                return;
            }

            bool ok = handler.HandleLocal(request);
            if (request.RequestsReceipt())
                _clusterSender.SendReceipt(ok, received.FromNetworkId, request.ReplyToJobId, request.ReplyToTaskId);
        }

        public void ReceiveReceiptFromRemote(ReceivedEvent received, CommandReceipt receipt)
        {
            if (string.IsNullOrEmpty(receipt.ReplyToJobId) || string.IsNullOrEmpty(receipt.ReplyToTaskId))
            {
                _log.LogWarning("Received job receipt from Node {0} without necessary job information", received.FromNodeId);
                return;
            }

            _log.LogDebug("Received receipt Job={0} Task={1} from node {2}", receipt.ReplyToJobId, receipt.ReplyToTaskId, received.FromNodeId);
            _data.Update<CommandJob>(receipt.ReplyToJobId, j => j.MarkTaskComplete(receipt.ReplyToTaskId, receipt.Success));
        }

        public CommandResponse DispatchCommandRequest(CommandRequest arg)
        {
            var handler = _commandHandlers.GetOrDefault(arg.Command);
            if (handler == null)
                return CommandResponse.Create(false);

            return handler.Handle(arg);
        }

        private class ClusterMessageSender : IClusterMessageSender
        {
            private readonly IMessageBus _messageBus;

            public ClusterMessageSender(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public void Send(ClusterMessage message)
            {
                _messageBus.Publish(ClusterMessage.SendEventName, message);
            }

            public void SendReceipt(bool success, string networkNodeId, string jobId, string taskId)
            {
                var message = new ClusterMessageBuilder()
                    .FromNode()
                    .ToNode(networkNodeId)
                    .WithObjectPayload(new CommandReceipt
                    {
                        Success = success,
                        ReplyToJobId = jobId,
                        ReplyToTaskId = taskId
                    })
                    .Build();
                _messageBus.Publish(ClusterMessage.SendEventName, message);
            }
        }
    }
}