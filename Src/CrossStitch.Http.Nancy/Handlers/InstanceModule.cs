using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Messages;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class InstanceModule : NancyModule
    {
        public InstanceModule(IMessageBus messageBus)
            : base("/instances")
        {
            Get["/"] = _ =>
            {
                return messageBus.Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(DataRequest<StitchInstance>.GetAll());
            };
            Get["/{Instance}"] = _ =>
            {
                string instance = _.Instance.ToString();
                var request = DataRequest<StitchInstance>.Get(instance);
                var response = messageBus.Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(request);
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