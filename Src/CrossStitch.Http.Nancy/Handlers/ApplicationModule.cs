﻿using Acquaintance;
using CrossStitch.App.Utility.Extensions;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Node.Messages;
using Nancy;
using Nancy.ModelBinding;
using System.Linq;

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
                return response.Responses.SelectMany(dr => dr.Entities.OrEmptyIfNull()).ToList();
            };

            Post["/"] = x =>
            {
                ApplicationChangeRequest request = this.Bind<ApplicationChangeRequest>();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, request).Responses.First();
            };

            // TODO: Move application management logic to a new handler, and send data requests through
            // that. 

            Get["/{Application}"] = x =>
            {
                string application = x.Application.ToString();
                var request = DataRequest<Application>.Get(application);
                var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
                return response.Responses.Select(dr => dr.Entity).ToList();
            };

            Put["/{Application}"] = x =>
            {
                var request = this.Bind<ApplicationChangeRequest>();
                request.Id = x.Application.ToString();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Update, request).Responses.First();
            };

            Delete["/{Application}"] = x =>
            {
                var request = new ApplicationChangeRequest
                {
                    Id = x.Application.ToString()
                };
                return messageBus.Request<ApplicationChangeRequest, GenericResponse>(ApplicationChangeRequest.Delete, request).Responses.First();
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
                    Application = x.Application,
                    Component = x.Component,
                    Contents = Request.Files.Single().Value
                };

                // TODO: Validate the file. It should be a .zip
                var response = messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(request);

                return response.Responses;
            };

            // TODO: Get Application/Component/Version
            // TODO: Delete Application/Component/Version

            Post["/{Application}/components/{Component}/versions/{Version}/createinstance"] = x =>
            {
                var request = new InstanceCreateRequest
                {
                    ApplicationId = x.Application,
                    ComponentId = x.Component,
                    VersionId = x.Version
                };

                HttpFile file = Request.Files.Single();

                return HttpStatusCode.OK;
            };
        }
    }
}
