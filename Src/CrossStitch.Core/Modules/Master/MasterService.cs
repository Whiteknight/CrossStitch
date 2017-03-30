using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
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
        private readonly JobManager _jobManager;

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
            _jobManager = new JobManager(_core.MessageBus, _data, _log);

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
            _jobManager.MarkTaskComplete(receipt.ReplyToJobId, receipt.ReplyToTaskId, receipt.Success);
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

        public CreateInstanceResponse CreateNewInstances(CreateInstanceRequest arg)
        {
            // If we are creating the instances locally, redirect to the Stitches module
            if (arg.LocalOnly)
            {
                var localResponse = _stitches.CreateInstances(arg, null, false);
                return new CreateInstanceResponse(localResponse);
            }

            // Otherwise we're distributing. First, get the recipient nodes.
            // TODO: A better algorithm for selecting nodes. Add instances to nodes with the lowest number of instances first.
            var nodes = _data.GetAll<NodeStatus>()
                .Where(ns => ns.RunningModules.Contains(ModuleNames.Stitches))
                .ToList();
            var selectedNodes = new List<NodeStatus>();
            for (int i = 0; i < arg.NumberOfInstances; i++)
            {
                // Do some scoring, to select the nodes with the most capacity
                var node = nodes[i % nodes.Count];
                selectedNodes.Add(node);
            }

            // Create a job, and start dispatching commands
            var job = new CommandJob();
            _data.Insert(job);

            foreach (var node in selectedNodes)
            {
                var task = job.CreateSubtask(CommandType.CreateStitchInstance, node.Id, node.Id);
                var payload = new CreateInstanceRequest
                {
                    Adaptor = arg.Adaptor,
                    GroupName = arg.GroupName,
                    Name = arg.Name,
                    NumberOfInstances = 1,
                    JobId = job.Id,
                    TaskId = task.Id
                };
                bool isRemote = node.Id != _core.NodeId;
                var response = _stitches.CreateInstances(payload, node.NetworkNodeId, isRemote);
                if (!isRemote && response != null)
                    task.Status = response.IsSuccess ? JobStatusType.Success : JobStatusType.Failure;
            }
            _data.Save(job, true);
            return new CreateInstanceResponse
            {
                JobId = job.Id,
                IsSuccess = true
            };
        }

        public void CreateNewInstanceFromRemote(ReceivedEvent received, CreateInstanceRequest request)
        {
            var response = _stitches.CreateInstances(request, null, false);
            if (request.ReceiptRequested)
                _clusterSender.SendReceipt(response.IsSuccess, received.FromNetworkId, request.JobId, request.TaskId);
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

        public PackageFileUploadResponse UploadStitchPackageFile(StitchGroupName groupName, string filePath, PackageFileUploadRequest arg)
        {
            // Send this to all nodes which are running the Stitches module
            var nodes = _data.GetAll<NodeStatus>()
                .Where(n => n.Id != _core.NodeId)
                .Where(ns => ns.RunningModules.Contains(ModuleNames.Stitches))
                .ToList();
            if (nodes.Count == 0)
                return new PackageFileUploadResponse(true, groupName, filePath);

            var job = new CommandJob();
            _data.Insert(job);

            foreach (var node in nodes)
            {
                var task = job.CreateSubtask(CommandType.UploadPackageFile, node.Id, node.Id);
                _clusterSender.SendPackageFile(node.NetworkNodeId, groupName, arg.FileName, filePath, job.Id, task.Id);
            }

            _data.Save(job, true);
            return new PackageFileUploadResponse(true, groupName, filePath, job.Id);
        }
    }
}