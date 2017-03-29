using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using Nancy;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messages.Stitches;
using Nancy.ModelBinding;

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

            Get["/nodes/{NodeId}/stitches"] = _ => messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
            {
                NodeId = _.NodeId.ToString()
            });

            Get["/nodes/{NodeId}/stitches/{StitchId}"] = _ => messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
            {
                NodeId = _.NodeId.ToString(),
                StitchId = _.StitchId.ToString()
            }).FirstOrDefault();

            Get["/stitches"] = _ => messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest());

            Get["/stitchgroups/{GroupName}"] = _ => messageBus.Request<StitchSummaryRequest, List<StitchSummary>>(new StitchSummaryRequest
            {
                StitchGroupName = _.GroupName.ToString()
            });

            Post["/stitchgroups/{GroupName}/upload"] = x =>
            {
                var file = Request.Files.Single();
                var request = new PackageFileUploadRequest
                {
                    GroupName = new StitchGroupName(x.GroupName.ToString()),
                    FileName = file.Name,
                    Contents = file.Value,
                    LocalOnly = false
                };

                return messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(request);
            };

            Post["/stitchgroups/{GroupName}/createinstance"] = x =>
            {
                var request = this.Bind<CreateInstanceRequest>();
                request.GroupName = new StitchGroupName(x.GroupName.ToString());
                request.LocalOnly = false;
                return messageBus.Request<CreateInstanceRequest, CreateInstanceResponse>(request);
            };

            Post["/stitchgroups/{GroupName}/stopall"] = _ => messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
            {
                Command = CommandType.StopStitchGroup,
                Target = _.GroupName.ToString()
            });

            Post["/stitchgroups/{GroupName}/startall"] = _ => messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
            {
                Command = CommandType.StartStitchGroup,
                Target = _.GroupName.ToString()
            });

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
