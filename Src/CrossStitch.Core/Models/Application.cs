using System;
using System.Collections.Generic;
using System.Linq;

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
        public Guid NodeId { get; set; }
        public List<ApplicationComponent> Components { get; set; }

        public void AddVersion(string componentName, string versionName)
        {
            var component = Components.FirstOrDefault(c => c.Name == componentName);
            if (component == null)
                return;

            component.AddVersion(versionName);
        }

        public bool HasVersion(string componentName, string versionName)
        {
            var component = Components.FirstOrDefault(c => c.Name == componentName);
            if (component == null)
                return false;

            return component.HasVersion(versionName);
        }
    }

    public class ApplicationComponent
    {
        public ApplicationComponent()
        {
            Versions = new List<ApplicationComponentVersion>();
        }

        public string Name { get; set; }
        public List<ApplicationComponentVersion> Versions { get; set; }

        public void AddVersion(string versionId)
        {
            if (Versions == null)
                Versions = new List<ApplicationComponentVersion>();

            var version = Versions.FirstOrDefault(v => v.Version == versionId);
            if (version != null)
                return;

            Versions.Add(new ApplicationComponentVersion
            {
                Version = versionId
            });
        }

        public bool HasVersion(string version)
        {
            if (Versions == null || !Versions.Any(v => v.Version == version))
                return false;
            return true;
        }
    }

    public class ApplicationComponentVersion
    {
        public string Version { get; set; }
    }
}
