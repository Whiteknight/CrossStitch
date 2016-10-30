using System.Collections.Generic;

namespace CrossStitch.Core.Data.Entities
{
    public class Application : IDataEntity
    {
        public Application()
        {
            Components = new List<ApplicationComponent>();
        }

        public string Id { get; set; }
        public long StoreVersion { get; set; }
        public string Name { get; set; }
        public List<ApplicationComponent> Components { get; set; }
    }

    public class ApplicationComponent
    {
        public ApplicationComponent()
        {
            Versions = new List<ApplicationComponentVersion>();
        }

        public string Name { get; set; }
        public List<ApplicationComponentVersion> Versions { get; set; }
    }

    public class ApplicationComponentVersion
    {
        public string Version { get; set; }
    }
}
