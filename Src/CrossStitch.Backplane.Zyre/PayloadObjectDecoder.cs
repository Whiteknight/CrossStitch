using Acquaintance.PubSub;
using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Core.Messages.Backplane;
using System;

namespace CrossStitch.Backplane.Zyre
{
    public class PayloadObjectDecoder
    {
        public IPublishableMessage DecodePayloadObject(string channel, MessageEnvelope envelope)
        {
            var objectType = envelope.PayloadObject.GetType();

            // Get the  ObjectsReceivedEvent<> object and .Object property
            var eventType = typeof(ObjectsReceivedEvent<>).MakeGenericType(objectType);
            var eventObject = Activator.CreateInstance(eventType);
            var objectsProperty = eventType.GetProperty(nameof(ObjectsReceivedEvent<object>.Object));

            objectsProperty.SetValue(eventObject, envelope.PayloadObject);

            envelope.Header.PopulateReceivedEvent(eventObject as ReceivedEvent);

            return new PublishableMessage(channel, eventType, eventObject);
        }
    }
}
