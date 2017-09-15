﻿using Acquaintance;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using Nancy;
using Nancy.ModelBinding;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class StitchGroupNancyModule : NancyModule
    {
        public StitchGroupNancyModule(IMessageBus messageBus)
            : base("/stitchgroups")
        {
            Post<PackageFileUploadResponse>("/{GroupName}/upload", x =>
            {
                var file = Request.Files.Single();
                var request = new PackageFileUploadRequest
                {
                    GroupName = new StitchGroupName(x.GroupName.ToString()),
                    FileName = file.Name,
                    Contents = file.Value,
                    LocalOnly = true
                };

                return messageBus.RequestWait<PackageFileUploadRequest, PackageFileUploadResponse>(request);
            });

            Post<CreateInstanceResponse>("/{GroupName}/createinstance", x =>
            {
                var request = this.Bind<CreateInstanceRequest>();
                request.GroupName = new StitchGroupName(x.GroupName.ToString());
                request.LocalOnly = true;
                return messageBus.RequestWait<CreateInstanceRequest, CreateInstanceResponse>(request);
            });

            // TODO: The rest of these requests are local-only, so we need to tell the handler
            // (MasterModule) that these requests are local and the similar-looking ones from the
            // Cluster api are cluster-wide.

            //Get("/{GroupName}", _ =>
            //{
            //    // TODO: Get all instances in the group, including status and home node
            //    return null;
            //});

            Post<CommandResponse>("/{GroupName}/stopall", _ => messageBus.RequestWait<CommandRequest, CommandResponse>(new CommandRequest
            {
                Command = CommandType.StopStitchGroup,
                Target = _.GroupName.ToString()
            }));

            Post<CommandResponse>("/{GroupName}/startall", _ => messageBus.RequestWait<CommandRequest, CommandResponse>(new CommandRequest
            {
                Command = CommandType.StartStitchGroup,
                Target = _.GroupName.ToString()
            }));

            //Post("/{GroupName}/stopoldversions", _ =>
            //{
            //    // TODO: Stop all instances in the version group which are older than the group
            //    // specified
            //    return null;
            //});
        }
    }
}
