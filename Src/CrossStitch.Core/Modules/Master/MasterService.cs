using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Handlers;
using CrossStitch.Core.Modules.Master.Models;
using CrossStitch.Core.Utility;
using CrossStitch.Core.Utility.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Master
{
    public class MasterService
    {
        private readonly CrossStitchCore _core;
        private readonly IModuleLog _log;
        private readonly IStitchRequestHandler _stitches;
        private readonly IClusterMessageSender _clusterSender;
        private readonly MasterDataRepository _data;
        private readonly Dictionary<CommandType, ICommandHandler> _commandHandlers;

        private string _networkNodeId;
        private string[] _clusterZones;

        public MasterService(CrossStitchCore core, IModuleLog log, MasterDataRepository data, IStitchRequestHandler stitches, IClusterMessageSender clusterSender)
        {
            _core = core;
            _log = log;
            _data = data;
            _stitches = stitches;
            _clusterSender = clusterSender;
            _clusterZones = new string[0];

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
            var stitches = _data.GetAll<StitchInstance>();
            return new NodeStatusBuilder(_core.NodeId, _core.Name, _networkNodeId, _clusterZones, _core.Modules.AddedModules, stitches).Build();
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
            message.FromNodeId = _core.NodeId;
            message.FromNetworkId = _networkNodeId;

            var stitches = _data.GetAllStitchSummaries();
            var messages = new DataMessageAddresser(stitches).AddressMessage(message);
            foreach (var outMessage in messages)
            {
                bool isRemote = !string.IsNullOrEmpty(outMessage.ToNodeId) && outMessage.ToNodeId != _core.NodeId;
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

        public List<StitchSummary> GetStitchSummaries(StitchSummaryRequest request)
        {
            IEnumerable<StitchSummary> query = _data.GetAllStitchSummaries();
            if (!string.IsNullOrEmpty(request.NodeId))
                query = query.Where(ss => ss.NodeId == request.NodeId);
            if (!string.IsNullOrEmpty(request.StitchGroupName))
            {
                var name = new StitchGroupName(request.StitchGroupName);
                query = query.Where(ss => name.Contains(ss.GroupName));
            }
            if (!string.IsNullOrEmpty(request.StitchId))
                query = query.Where(ss => ss.Id == request.StitchId);

            return query.ToList();
        }

        public CommandResponse DispatchCommandRequest(CommandRequest arg)
        {
            var handler = _commandHandlers.GetOrDefault(arg.Command);
            if (handler == null)
                return CommandResponse.Create(false);

            return handler.Handle(arg);
        }

        public void SetNetworkNodeId(string networkNodeId)
        {
            _networkNodeId = networkNodeId;
            _log.LogDebug("Registered with cluster as NodeId={0}", _networkNodeId);
        }

        public void SetClusterZones(string[] zones)
        {
            _clusterZones = zones ?? new string[0];
            if (_clusterZones.Length > 0)
                _log.LogDebug("Member of cluster zones {0}", string.Join(",", _clusterZones));
        }
    }
}