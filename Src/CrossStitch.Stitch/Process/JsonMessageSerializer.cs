using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.Process
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        public FromStitchMessage DeserializeFromStitchMessage(string message)
        {
            return JsonUtility.Deserialize<FromStitchMessage>(message);
        }

        public ToStitchMessage DeserializeToStitchMessage(string message)
        {
            return JsonUtility.Deserialize<ToStitchMessage>(message);
        }

        public string Serialize(ToStitchMessage message)
        {
            return JsonUtility.Serialize(message);
        }

        public string Serialize(FromStitchMessage message)
        {
            return JsonUtility.Serialize(message);
        }
    }
}
