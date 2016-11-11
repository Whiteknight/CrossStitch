using Acquaintance;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Data.Messages;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
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