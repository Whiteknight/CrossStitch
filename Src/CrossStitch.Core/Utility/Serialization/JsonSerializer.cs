using System.Text;
using Newtonsoft.Json;

namespace CrossStitch.Core.Utility.Serialization
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public byte[] Serialize<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data, _settings);
            return Encoding.Unicode.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            string json = Encoding.Unicode.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public object DeserializeObject(byte[] bytes)
        {
            string json = Encoding.Unicode.GetString(bytes);
            return JsonConvert.DeserializeObject(json, _settings);
        }
    }
}