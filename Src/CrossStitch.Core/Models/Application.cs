using CrossStitch.Core.Modules.Data;
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
        public string NodeId { get; set; }
        public List<ApplicationComponent> Components { get; set; }

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
}
