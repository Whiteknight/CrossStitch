namespace CrossStitch.Core.Data
{
    public interface IDataEntity
    {
        string Id { get; set; }
        string Name { get; set; }
        long StoreVersion { get; set; }
    }
}
