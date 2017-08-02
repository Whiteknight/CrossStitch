namespace CrossStitch.Core.Utility.Serialization
{
    public interface IByteSerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] bytes);
        object DeserializeObject(byte[] bytes);
    }
}
