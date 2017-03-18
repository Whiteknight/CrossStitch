using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using Nancy;
using Nancy.ModelBinding;
using System.Linq;

namespace CrossStitch.Http.NancyFx.Handlers
{
    public class StitchGroupNancyModule : NancyModule
    {
        public StitchGroupNancyModule(IMessageBus messageBus)
            : base("/stitchgroups")
        {
            Get["/{GroupName}"] = _ =>
            {
                // TODO: Get all instances in the group, including status and home node
                return null;
            };

            Post["/{GroupName}/stopall"] = _ =>
            {
                // TODO: Stop all instances in the group
                return null;
            };

            Post["/{GroupName}/stopoldversions"] = _ =>
            {
                // TODO: Stop all instances in the version group which are older than the group
                // specified
                return null;
            };

            Post["/{GroupName}/rebalance"] = _ =>
            {
                // TODO: Rebalance all instances in the group across the cluster
                return null;
            };
        }
    }
}
