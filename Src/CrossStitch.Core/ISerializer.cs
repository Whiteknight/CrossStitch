using System;
using System.Text;
using Newtonsoft.Json;

namespace CrossStitch.Core
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] bytes);
        object DeserializeObject(Type type, byte[] bytes);
    }

    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings {
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

        public object DeserializeObject(Type type, byte[] bytes)
        {
            string json = Encoding.Unicode.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
    }
}
