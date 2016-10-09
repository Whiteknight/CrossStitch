namespace CrossStitch.Core.Messaging.PubSub
{
    public interface IPubSubSubscription<in TPayload>
    {
        void Publish(TPayload payload);
    }
}