using Acquaintance;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchEventObserver : IStitchEventObserver
    {
        private readonly IMessageBus _messageBus;
        private readonly IModuleLog _log;

        public StitchEventObserver(IMessageBus messageBus, IModuleLog log)
        {
            _messageBus = messageBus;
            _log = log;
        }

        public void StitchInstancesOnStitchStateChanged(string instanceId, bool isRunning, bool wasRequested)
        {
            var channel = isRunning ? StitchInstanceEvent.ChannelStarted : StitchInstanceEvent.ChannelStopped;
            _messageBus.Publish(channel, new StitchInstanceEvent
            {
                InstanceId = instanceId
            });

            _log.LogInformation("Stitch instance {0} is {1}", instanceId, isRunning ? "started" : "stopped");
        }

        public void StitchInstanceManagerOnRequestResponseReceived(string instanceId, long messageId, bool success)
        {
            // TODO: How to report errors here?
        }

        public void StitchInstanceManagerOnLogsReceived(string instanceId, string[] logs)
        {
            // TODO: Should get the StitchInstance from the data store and enrich this message?
            foreach (var s in logs)
                _log.LogInformation("Stitch Id={0} Log Message: {1}", instanceId, s);
        }

        public void StitchInstanceManagerOnHeartbeatReceived(string instanceId, long heartbeatId)
        {
            _messageBus.Publish(StitchInstanceEvent.ChannelSynced, new StitchInstanceEvent
            {
                InstanceId = instanceId,
                DataId = heartbeatId
            });
        }

        public void StitchInstanceManagerOnDataMessageReceived(string instanceId, long messageId, string toGroup, string toInstanceId, string channelName, string data)
        {
            _log.LogDebug("Received data message Id={0} from StitchInstanceId={1}", messageId, instanceId);
            _messageBus.Publish(new StitchDataMessage
            {
                DataChannelName = channelName,
                Data = data,
                FromStitchInstanceId = instanceId,
                Id = messageId,
                ToStitchGroup = toGroup,
                ToStitchInstanceId = toInstanceId
            });
        }
    }
}
