using Acquaintance;
using CrossStitch.Core.Messages.Alerts;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Alerts
{
    public class AlertsModule : IModule
    {
        private readonly IAlertSender[] _senders;
        private readonly IMessageBus _messageBus;
        private SubscriptionCollection _subscriptions;

        public AlertsModule(CrossStitchCore core, params IAlertSender[] senders)
        {
            _senders = senders;
            _messageBus = core.MessageBus;
        }

        public string Name => ModuleNames.Alerts;

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new Dictionary<string, string>
            {
                { "Senders", _senders.Length.ToString() }
            };
        }

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Subscribe<AlertEvent>(b => b
                .WithDefaultTopic()
                .Invoke(ReceiveAlert));
        }

        public void Stop()
        {
            _subscriptions.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
        }

        private void ReceiveAlert(AlertEvent obj)
        {
            // TODO: We need some kind of rules engine here, so we can keep track of which events
            // we want to monitor at which severity level, and what threshold.

            // TODO: We need some way to send in configuration values from the user, such as through
            // the HTTP API. A way to take in command strings and parse them might be the only
            // generic option.

            // TODO: Add the alert to some kind of storage, by severity/key
            // If the number of events in that key exceed a configurable threshold  is crossed, 
            // package those up into an output format and send it to all IAlertSenders
            // Requires some kind of data storage. Should we use the DataModule?
        }
    }

    public interface IAlertSender
    {

    }
}
