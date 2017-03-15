using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;
using Nancy;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class StitchNancyModule : NancyModule
    {
        // TODO: /stitchgroups/* endpoints where we can perform actions on entire groups
        public StitchNancyModule(IMessageBus messageBus)
            : base("/stitches")
        {
            var data = new DataHelperClient(messageBus);

            Get["/"] = _ => data.GetAll<StitchInstance>().ToList();

            Get["/{StitchId}"] = _ => data.Get<StitchInstance>(_.StitchId.ToString());

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