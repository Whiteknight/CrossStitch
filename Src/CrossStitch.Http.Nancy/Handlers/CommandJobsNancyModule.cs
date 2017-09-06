using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Models;
using Nancy;

namespace CrossStitch.Http.NancyFx.Handlers
{

    public class CommandJobsNancyModule: NancyModule
    {
        public CommandJobsNancyModule(IMessageBus messageBus)
            : base("/commandjobs")
        {
            var data = new DataHelperClient(messageBus);

            Get("/{JobId}", _ => data.Get<CommandJob>(_.JobId.ToString()));
        }
    }
}