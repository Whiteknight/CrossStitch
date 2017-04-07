using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master.Models;
using Nancy;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using CrossStitch.Core.Messages.Security;
using CrossStitch.Core.Messages.Stitches;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Security;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class ClusterNancyModule : NancyModule
    {
        public ClusterNancyModule(IMessageBus messageBus)
            : base("/cluster")
        {
            var data = new DataHelperClient(messageBus);

            Before.AddItemToEndOfPipeline(ctx => AuthenticateUser(messageBus, ctx));

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

        private Response AuthenticateUser(IMessageBus messageBus, NancyContext context)
        {
            if (context.CurrentUser != null)
                return null;

            // TODO: Map method to SecurityRequestType
            var method = context.Request.Method;
            var type = SecurityRequestType.Read;
            var response = messageBus.Request<SecurityRequest, SecurityResponse>(new SecurityRequest
            {
                Type = type
            });

            if (response != null && response.Allowed)
            {
                context.CurrentUser = new CrossStitchHttpUserIdentity
                {
                    UserName = response.UserName,
                    Claims = new List<string>()
                };
            }

            return new HtmlResponse(HttpStatusCode.Unauthorized);
        }

        private class CrossStitchHttpUserIdentity : IUserIdentity
        {
            public string UserName { get; set; }
            public IEnumerable<string> Claims { get; set; }
        }
    }
}
