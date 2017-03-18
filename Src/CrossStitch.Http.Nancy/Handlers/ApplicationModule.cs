using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using Nancy;
using Nancy.ModelBinding;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class ApplicationNancyModule : NancyModule
    {
        public ApplicationNancyModule(IMessageBus messageBus)
            : base("/applications")
        {
            var data = new DataHelperClient(messageBus);

            Get["/"] = x => data.GetAll<Application>().ToList();

            Post["/"] = x =>
            {
                var request = this.Bind<ApplicationChangeRequest>();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, request);
            };

            Get["/{Application}"] = x => data.Get<Application>(x.Application.ToString());

            Put["/{Application}"] = x =>
            {
                var request = this.Bind<ApplicationChangeRequest>();
                request.Id = x.Application.ToString();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Update, request);
            };

            Delete["/{Application}"] = x =>
            {
                var request = new ApplicationChangeRequest
                {
                    Id = x.Application.ToString()
                };
                return messageBus.Request<ApplicationChangeRequest, GenericResponse>(ApplicationChangeRequest.Delete, request);
            };

            Post["/{Application}/components"] = x =>
            {
                var request = this.Bind<ComponentChangeRequest>();
                request.Application = x.Application.ToString();
                return messageBus.Request<ComponentChangeRequest, GenericResponse>(ComponentChangeRequest.Insert, request);
            };

            Delete["/{Application}/components/{Component}"] = x =>
            {
                var request = new ComponentChangeRequest();
                request.Application = x.Application.ToString();
                request.Name = x.Component.ToString();
                return messageBus.Request<ComponentChangeRequest, GenericResponse>(ComponentChangeRequest.Delete, request);
            };

            Post["/{Application}/components/{Component}/upload"] = x =>
            {
                var request = new PackageFileUploadRequest
                {
                    ApplicationId = x.Application,
                    Component = x.Component,
                    Contents = Request.Files.Single().Value
                };

                return messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(request);
            };

            // TODO: Get Application/Component/Version
            // TODO: Delete Application/Component/Version

            Post["/{Application}/components/{Component}/versions/{Version}/createinstance"] = x =>
            {
                // TODO: Don't use StitchInstance as the DTO here. Use a smaller object with just
                // the fields we need, validate that we have everything, and then map to a
                // StitchInstance (in the RequestCoordinator?)
                var request = this.Bind<CreateInstanceRequest>();
                request.GroupName = new StitchGroupName(x.Application.ToString(), x.Component.ToString(), x.Version.ToString());
                return messageBus.Request<CreateInstanceRequest, InstanceResponse>(InstanceRequest.ChannelCreate, request);
            };
        }
    }
}
