using Acquaintance;
using CrossStitch.App.Utility.Extensions;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
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
                Application application = this.Bind<Application>();
                application.StoreVersion = 0;
                foreach (var component in application.Components.OrEmptyIfNull())
                    component.Versions = null;
                var request = DataRequest<Application>.Save(application);
                var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
                return response.Responses.Select(dr => dr.Entity).ToList();
            };

            // TODO: Delete application

            // TODO: Figure out what we want here, because I don't think we want to just overwrite
            // all information about components and versions. We probably want to merge only certain
            // fields.
            //Put["/"] = x =>
            //{
            //    Application application = this.Bind<Application>();
            //    foreach (var component in application.Components.OrEmptyIfNull())
            //        component.Versions = null;
            //    var request = DataRequest<Application>.Save(application);
            //    var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
            //    return null;
            //};

            Get["/{Application}"] = x =>
            {
                string application = x.Application.ToString();
                var request = DataRequest<Application>.Get(application);
                var response = messageBus.Request<DataRequest<Application>, DataResponse<Application>>(request);
                return response.Responses.Select(dr => dr.Entity).ToList();
            };

            // TODO: Post new Application/Component
            // TODO: Delete Application/Component

            Post["/{Application}/components/{Component}/upload"] = x =>
            {
                var request = new PackageFileUploadRequest();
                request.ApplicationId = x.Application;
                request.ComponentId = x.Component;
                request.Contents = Request.Files.Single().Value;

                var response = messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(request);

                return response.Responses;
            };

            // TODO: Get Application/Component/Version
            // TODO: Delete Application/Component/Version

            Post["/{Application}/components/{Component}/versions/{Version}/createinstance"] = x =>
            {
                var request = new InstanceCreateRequest();
                request.ApplicationId = x.Application;
                request.ComponentId = x.Component;
                request.VersionId = x.Version;

                HttpFile file = Request.Files.Single();

                return HttpStatusCode.OK;
            };
        }
    }
}
