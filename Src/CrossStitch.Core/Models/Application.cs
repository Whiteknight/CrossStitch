using CrossStitch.Core.Modules.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Models
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
        public string Zone { get; set; }
        public Guid NodeId { get; set; }
        public List<ApplicationComponent> Components { get; set; }

        public static string VersionFullName(string applicationId, string component, string version)
        {
            return $"{applicationId}.{component}.{version}";
        }

        public bool AddComponent(string name)
        {
            var component = Components.FirstOrDefault(c => c.Name == name);
            if (component != null)
                return false;

            Components.Add(new ApplicationComponent
            {
                Name = name,
                FullName = Name + "." + name
            });
            return true;
        }

        public bool RemoveComponent(string name)
        {
            Components = Components.Where(c => c.Name != name).ToList();
            return true;
        }

        public void AddVersion(string componentName, string versionName)
        {
            var component = Components.FirstOrDefault(c => c.Name == componentName);

            component?.AddVersion(versionName);
        }

        public bool HasVersion(string componentName, string versionName)
        {
            var component = Components.FirstOrDefault(c => c.Name == componentName);
            return component != null && component.HasVersion(versionName);
        }
    }

    public class ApplicationComponent
    {
        public ApplicationComponent()
        {
            Versions = new List<ApplicationComponentVersion>();
        }

        public string Name { get; set; }
        public string FullName { get; set; }

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
                Version = versionId,
                FullName = FullName + "." + versionId
            });
        }

        public bool HasVersion(string version)
        {
            if (Versions == null || Versions.All(v => v.Version != version))
                return false;
            return true;
        }
    }

    public class ApplicationComponentVersion
    {
        public string Version { get; set; }
        public string FullName { get; set; }
    }
}
