using Acquaintance;
using Nancy;
using Nancy.ModelBinding;
using System.Linq;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility.Extensions;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class ApplicationModule : NancyModule
    {
        public ApplicationModule(IMessageBus messageBus)
            : base("/applications")
        {
            Get["/"] = x =>
            {
                var request = DataRequest<Application>.GetAll();
                var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
                return response.Entities.OrEmptyIfNull().ToList();
            };

            Post["/"] = x =>
            {
                ApplicationChangeRequest request = this.Bind<ApplicationChangeRequest>();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, request);
            };

            Get["/{Application}"] = x =>
            {
                string application = x.Application.ToString();
                var request = DataRequest<Application>.Get(application);
                var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
                return response.Entity;
            };

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

                // TODO: Validate the file. It should be a .zip
                return messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(request);
            };

            // TODO: Get Application/Component/Version
            // TODO: Delete Application/Component/Version

            Post["/{Application}/components/{Component}/versions/{Version}/createinstance"] = x =>
            {
                StitchInstance stitchInstance = this.Bind<StitchInstance>();
                stitchInstance.Application = x.Application.ToString();
                stitchInstance.Component = x.Component.ToString();
                stitchInstance.Version = x.Version.ToString();

                return messageBus.Request<StitchInstance, StitchInstance>(StitchInstance.ChannelCreate, stitchInstance);
            };
        }
    }
}
