using Acquaintance;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch.V1.Core;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchEventObserver
    {
        private readonly IMessageBus _messageBus;
        private readonly IModuleLog _log;

        public StitchEventObserver(IMessageBus messageBus, IModuleLog log)
        {
            _messageBus = messageBus;
            _log = log;
        }

        public void StitchInstancesOnStitchStateChanged(object sender, StitchProcessEventArgs e)
        {
            var channel = e.IsRunning ? StitchInstanceEvent.ChannelStarted : StitchInstanceEvent.ChannelStopped;
            _messageBus.Publish(channel, new StitchInstanceEvent
            {
                InstanceId = e.InstanceId
            });

            _log.LogInformation("Stitch instance {0} is {1}", e.InstanceId, e.IsRunning ? "started" : "stopped");
        }

        public void StitchInstanceManagerOnRequestResponseReceived(object sender, RequestResponseReceivedEventArgs e)
        {
            // TODO: How to report errors here?
        }

        public void StitchInstanceManagerOnLogsReceived(object sender, LogsReceivedEventArgs e)
        {
            // TODO: Should get the StitchInstance from the data store and enrich this message?
            foreach (var s in e.Logs)
                _log.LogInformation("Stitch Id={0} Log Message: {1}", e.StitchInstanceId, s);
        }

        public void StitchInstanceManagerOnHeartbeatReceived(object sender, HeartbeatSyncReceivedEventArgs e)
        {
            _messageBus.Publish(StitchInstanceEvent.ChannelSynced, new StitchInstanceEvent
            {
                InstanceId = e.StitchInstanceId,
                DataId = e.Id
            });
        }

        public void StitchInstanceManagerOnDataMessageReceived(object sender, DataMessageReceivedEventArgs e)
        {
            _log.LogDebug("Received data message Id={0} from StitchInstanceId={1}", e.MessageId, e.FromStitchInstanceId);
            _messageBus.Publish(new StitchDataMessage
            {
                DataChannelName = e.ChannelName,
                Data = e.Data,
                FromStitchInstanceId = e.FromStitchInstanceId,
                Id = e.MessageId,
                ToStitchGroup = e.ToGroupName,
                ToStitchInstanceId = e.ToStitchInstanceId
            });
        }
    }
}
