namespace CrossStitch.Core.Models
{
    public class StitchGroupName
    {
        public StitchGroupName(string versionString)
        {
            VersionString = versionString;
            string[] parts = versionString.Split('.');
            if (parts.Length >= 1)
                ApplicationId = parts[0];
            if (parts.Length >= 2)
                Component = parts[1];
            if (parts.Length == 3)
                Version = parts[2];
        }

        public StitchGroupName(string applicationId, string component, string version)
        {
            ApplicationId = applicationId;
            Component = component;
            Version = version;

            if (string.IsNullOrEmpty(applicationId))
                VersionString = "";
            else if (string.IsNullOrEmpty(component))
                VersionString = applicationId;
            else if (string.IsNullOrEmpty(version))
                VersionString = $"{applicationId}.{component}";
            else
                VersionString = $"{applicationId}.{component}.{version}";
        }

        public string ApplicationId { get; }
        public string Component { get; }
        public string Version { get; }
        public string VersionString { get; }

        public override string ToString()
        {
            return VersionString;
        }

        public bool IsApplicationGroup()
        {
            return !string.IsNullOrEmpty(ApplicationId) && string.IsNullOrEmpty(Component);
        }

        public bool IsComponentGroup()
        {
            return !string.IsNullOrEmpty(ApplicationId) && !string.IsNullOrEmpty(Component) && string.IsNullOrEmpty(Version);
        }

        public bool IsVersionGroup()
        {
            return !string.IsNullOrEmpty(ApplicationId) && !string.IsNullOrEmpty(Component) && !string.IsNullOrEmpty(Version);
        }

        public bool Contains(StitchGroupName otherGroup)
        {
            if (ApplicationId != otherGroup.ApplicationId)
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
