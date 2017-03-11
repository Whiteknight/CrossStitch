namespace CrossStitch.Stitch.Utility
{
    public static class JsonUtility
    {
        private static Newtonsoft.Json.JsonSerializerSettings _settings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
        };

        public static string Serialize<T>(T data)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(data, _settings);
        }

        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public static object DeserializeObject(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, _settings);
        }
    }
}
