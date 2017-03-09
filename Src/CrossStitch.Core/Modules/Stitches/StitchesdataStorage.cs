using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchesDataStorage : DataHelperClient
    {
        public StitchesDataStorage(IMessageBus messageBus)
            : base(messageBus)
        {
        }

        public StitchInstance GetInstance(string id)
        {
            return Get<StitchInstance>(id);
        }
    }
}
