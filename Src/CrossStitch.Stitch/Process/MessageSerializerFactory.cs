using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.Process
{
    public class MessageSerializerFactory : IFactory<IMessageSerializer, MessageSerializerType>
    {
        public IMessageSerializer Create(MessageSerializerType arg)
        {
            return new JsonMessageSerializer();
        }
    }
}