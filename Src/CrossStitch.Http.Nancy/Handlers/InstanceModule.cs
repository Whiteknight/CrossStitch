using Acquaintance;
using CrossStitch.Core.Messages.Core;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class StitchNancyModule : NancyModule
    {
        public StitchNancyModule(IMessageBus messageBus)
            : base("/stitches")
        {
            Get["/"] = _ =>
            {
                return messageBus.Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(DataRequest<StitchInstance>.GetAll());
            };

            Get["/{StitchId}"] = _ =>
            {
                string instance = _.StitchId.ToString();
                var request = DataRequest<StitchInstance>.Get(instance);
                var response = messageBus.Request<DataRequest<StitchInstance>, DataResponse<StitchInstance>>(request);
                return response.Entity;
            };

            Post["/{StitchId}/start"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
                {
                    Id = _.StitchId.ToString()
                });
            };

            Post["/{StitchId}/stop"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStop, new InstanceRequest
                {
                    Id = _.StitchId.ToString()
                });
            };

            Post["/{StitchId}/clone"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelClone, new InstanceRequest
                {
                    Id = _.StitchId.ToString()
                });
            };

            Delete["/{StitchId}"] = _ =>
            {
                return messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelDelete, new InstanceRequest
                {
                    Id = _.StitchId.ToString()
                });
            };

            Get["/{StitchId}/status"] = _ =>
            {
                return messageBus.Request<StitchHealthRequest, StitchHealthResponse>(new StitchHealthRequest
                {
                    StitchId = _.StitchId.ToString()
                });
            };
        }
    }
}