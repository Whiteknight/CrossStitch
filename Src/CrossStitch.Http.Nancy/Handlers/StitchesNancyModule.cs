using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.StitchMonitor;
using CrossStitch.Core.Models;
using Nancy;
using System.Linq;
using CrossStitch.Core.Messages.Stitches;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class StitchesNancyModule : NancyModule
    {
        public StitchesNancyModule(IMessageBus messageBus)
            : base("/stitches")
        {
            var data = new DataHelperClient(messageBus);

            // TODO: Method to get all StitchSummaries from the entire cluster (MasterModule)
            Get["/"] = _ => data.GetAll<StitchInstance>().ToList();

            Get["/{StitchId}"] = _ => data.Get<StitchInstance>(_.StitchId.ToString());

            Post["/{StitchId}/start"] = _ =>
            {
                return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
                {
                    Command = CommandType.StartStitchInstance,
                    Target = _.StitchId.ToString()
                });
            };

            Post["/{StitchId}/stop"] = _ =>
            {
                return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
                {
                    Command = CommandType.StopStitchInstance,
                    Target = _.StitchId.ToString()
                });
            };

            Get["/{StitchId}/logs"] = _ =>
            {
                // Get last N log messages from the stitch
                return null;
            };

            Get["/{StitchId}/resources"] = _ =>
            {
                return messageBus.Request<StitchResourceUsageRequest, StitchResourceUsage>(new StitchResourceUsageRequest
                {
                    StitchInstanceId = _.StitchId.ToString()
                });
            };

            //Post["/{StitchId}/clone"] = _ =>
            //{
            //    return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
            //    {
            //        Command = CommandType.CloneStitchInstance,
            //        Target = _.StitchId.ToString()
            //    });
            //};

            Delete["/{StitchId}"] = _ =>
            {
                return messageBus.Request<CommandRequest, CommandResponse>(new CommandRequest
                {
                    Command = CommandType.RemoveStitchInstance,
                    Target = _.StitchId.ToString()
                });
            };

            Get["/{StitchId}/status"] = _ =>
            {
                return messageBus.Request<StitchHealthRequest, StitchHealthResponse>(new StitchHealthRequest
                {
                    StitchId = _.StitchId.ToString()
                });
            };

            Post["/{StitchId}/moveto/{NodeId}"] = _ =>
            {
                // TODO: Move the stitch instance to the specified node
                return null;
            };
        }
    }
}