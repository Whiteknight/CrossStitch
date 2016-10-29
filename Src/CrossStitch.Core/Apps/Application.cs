using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Apps
{
    public class ClientApplication
    {
        public ClientApplication(Guid id, string name)
        {
            Id = id;
            Name = name;
            Components = new Dictionary<string, ApplicationComponent>();
        }

        // Represents a client application
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ApplicationComponent> Components { get; set; }
        
        public ApplicationComponent GetComponent(string name)
        {
            if (Components.ContainsKey(name))
                return Components[name];

            Guid id = Guid.NewGuid();
            ApplicationComponent component = new ApplicationComponent(this, id, name);
            Components.Add(name, component);
            return component;
        }

        

        
    }

    //public class ApplicationPackage
    //{
    //    public Guid Id { get; set; }
    //    public string Name { get; set; }
    //    public ICollection<ComponentVersion> Versions { get; set; }
    //}

    public class ApplicationComponent
    {
        
        // A client application may consist of multiple components. This class represents the type of component
        public ApplicationComponent(ClientApplication application, Guid id, string name)
        {
            Id = id;
            Name = name;
            Application = application;
            Versions = new Dictionary<string, ComponentVersion>();
        }

        public Guid Id { get; set; }
        public ClientApplication Application { get; private set; }
        public string Name { get; set; }
        public Dictionary<string, ComponentVersion> Versions { get; set; }

        public ComponentVersion GetVersion(string versionStr)
        {
            ComponentVersion version;
            if (Versions.ContainsKey(versionStr))
                version = Versions[versionStr];
            else
            {
                var versionId = Guid.NewGuid();
                version = new ComponentVersion(versionId, versionStr);
                Versions.Add(versionStr, version);
            }
            return version;
        }
    }

    public class ComponentVersion
    {
        public ComponentVersion(Guid id, string version)
        {
            Id = id;
            Version = version;
            Created = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public string Version { get; set; }
        public DateTime Created { get; set; }
    }

    

}