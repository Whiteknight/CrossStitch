namespace CrossStitch.Core.Models
{
    // TODO: We really want this class to be immutable, but we need to make the properties mutable
    // for now because of json serialization/deserialization concerns. We should fix this eventually.
    public class StitchGroupName
    {
        public StitchGroupName()
        {

        }

        public StitchGroupName(string versionString)
        {
            VersionString = versionString;
            string[] parts = versionString.Split('.');
            if (parts.Length >= 1)
                Application = parts[0];
            if (parts.Length >= 2)
                Component = parts[1];
            if (parts.Length == 3)
                Version = parts[2];
        }

        public StitchGroupName(string application, string component, string version)
        {
            Application = application;
            Component = component;
            Version = version;

            if (string.IsNullOrEmpty(application))
                VersionString = "";
            else if (string.IsNullOrEmpty(component))
                VersionString = application;
            else if (string.IsNullOrEmpty(version))
                VersionString = $"{application}.{component}";
            else
                VersionString = $"{application}.{component}.{version}";
        }

        public string Application { get; set; }
        public string Component { get; set; }
        public string Version { get; set; }
        public string VersionString { get; set; }

        public override string ToString()
        {
            return VersionString;
        }

        public bool IsValid()
        {
            return VersionString != null;
        }

        public bool IsApplicationGroup()
        {
            return !string.IsNullOrEmpty(Application) && string.IsNullOrEmpty(Component);
        }

        public bool IsComponentGroup()
        {
            return !string.IsNullOrEmpty(Application) && !string.IsNullOrEmpty(Component) && string.IsNullOrEmpty(Version);
        }

        public bool IsVersionGroup()
        {
            return !string.IsNullOrEmpty(Application) && !string.IsNullOrEmpty(Component) && !string.IsNullOrEmpty(Version);
        }

        public bool Contains(StitchGroupName otherGroup)
        {
            if (Application != otherGroup.Application)
                return false;
            if (IsApplicationGroup())
                return true;

            if (Component != otherGroup.Component)
                return false;
            if (IsComponentGroup())
                return true;

            return Version == otherGroup.Version;
        }

        public bool Contains(string otherGroupName)
        {
            return Contains(new StitchGroupName(otherGroupName));
        }

        // TODO: Equality members
    }
}
