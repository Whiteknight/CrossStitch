using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using Nancy;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class ClusterNancyModule : NancyModule
    {
        public ClusterNancyModule(IMessageBus messageBus)
            : base("/cluster")
        {
            var data = new DataHelperClient(messageBus);

            Get["/"] = _ => data.GetAll<NodeStatus>();

            Get["/nodes/{NodeId}"] = _ => data.Get<NodeStatus>(_.NodeId.ToString());

            Get["/nodes/{NodeId}/stitches"] = _ =>
            {
                return messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
                {
                    NodeId = _.NodeId.ToString()
                });
            };

            Get["/nodes/{NodeId}/stitches/{StitchId}"] = _ =>
            {
                return messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
                {
                    NodeId = _.NodeId.ToString(),
                    StitchId = _.StitchId.ToString()
                }).FirstOrDefault();
            };

            Get["/stitches"] = _ =>
            {
                return messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest());
            };

            Get["/stitchgroups/{GroupName}"] = _ =>
            {
                return messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
                {
                    StitchGroupName = _.GroupName.ToString()
                });
            };

            Post["/stitchgroups/{GroupName}/stopall"] = _ =>
            {
                return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
                {
                    Command = CommandType.StopStitchGroup,
                    Target = _.GroupName.ToString()
                });
            };

            Post["/stitchgroups/{GroupName}/startall"] = _ =>
            {
                return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
                {
                    Command = CommandType.StartStitchGroup,
                    Target = _.GroupName.ToString()
                });
            };

            Post["/stitchgroups/{GroupName}/stopoldversions"] = _ =>
            {
                // TODO: Stop all instances in the version group which are older than the group
                // specified
                return null;
            };

            Post["/stitchgroups/{GroupName}/rebalance"] = _ =>
            {
                // TODO: Rebalance all instances in the group across the cluster
                return null;
            };
        }
    }
}
