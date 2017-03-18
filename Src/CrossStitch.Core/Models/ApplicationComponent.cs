using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Models
{
    public class ApplicationComponent
    {
        public ApplicationComponent()
        {
            Versions = new List<ApplicationComponentVersion>();
        }

        public string Name { get; set; }
        public StitchGroupName GroupName { get; set; }

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
                GroupName = new StitchGroupName(GroupName.ApplicationId, GroupName.Component, versionId)
            });
        }

        public bool HasVersion(string version)
        {
            if (Versions == null || Versions.All(v => v.Version != version))
                return false;
            return true;
        }
    }
}
