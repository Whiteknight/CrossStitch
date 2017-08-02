namespace CrossStitch.Core.Models
{
    public class PackageFile : IDataEntity
    {
        public string Id { get; set; }
        public string Name => GroupName.ToString();
        public long StoreVersion { get; set; }

        public StitchGroupName GroupName { get; set; }
        public string FileName { get; set; }

        public InstanceAdaptorDetails Adaptor { get; set; }

    }
}
