using CrossStitch.Core.Utility;

namespace CrossStitch.Core.Messaging
{
    public static class MessageBusExtensions
    {
        public static void Publish<TPayload>(this IMessageBus messageBus, PayloadEventArgs<TPayload> eventArgs)
        {
            messageBus.Publish(eventArgs.Command, eventArgs.Payload);
        }
    }
}
