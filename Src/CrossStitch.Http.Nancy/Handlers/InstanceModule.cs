using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
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
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Post["/{Instance}/stop"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStop, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Post["/{Instance}/clone"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelClone, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };

            Delete["/{Instance}"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelDelete, new InstanceRequest
                {
                    Id = _.Instance.ToString()
                });
            };
        }
    }
}