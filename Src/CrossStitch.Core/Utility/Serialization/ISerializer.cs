namespace CrossStitch.App
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] bytes);
        object DeserializeObject(byte[] bytes);
    }
}
