using CrossStitch.Stitch.Utility;
using System.Text;

namespace CrossStitch.Core.Utility.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data)
        {
            var json = JsonUtility.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.Deserialize<T>(json);
        }

        public object DeserializeObject(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.DeserializeObject(json);
        }
    }
}