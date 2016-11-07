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
                return response.Entities.OrEmptyIfNull().ToList();
            };

            Post["/"] = x =>
            {
                ApplicationChangeRequest request = this.Bind<ApplicationChangeRequest>();
                return messageBus.Request<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, request);
            };

            // TODO: Move application management logic to a new handler, and send data requests through
            // that. 

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
                    Application = x.Application,
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
                Instance instance = this.Bind<Instance>();
                instance.Application = x.Application.ToString();
                instance.Component = x.Component.ToString();
                instance.Version = x.Version.ToString();

                return messageBus.Request<Instance, Instance>(Instance.CreateEvent, instance);
            };
        }
    }

    public class InstanceModule : NancyModule
    {
        public InstanceModule(IMessageBus messageBus)
            : base("/instances")
        {
            Get["/{Instance}"] = _ =>
            {
                string instance = _.Instance.ToString();
                var request = DataRequest<Instance>.Get(instance);
                var response = messageBus.Request<DataRequest<Instance>, DataResponse<Instance>>(request);
                return response.Entity;
            };
            Post["/{Instance}/start"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Start, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Post["/{Instance}/stop"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Stop, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Post["/{Instance}/clone"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Clone, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Delete["/{Instance}"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Delete, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };
        }
    }
}