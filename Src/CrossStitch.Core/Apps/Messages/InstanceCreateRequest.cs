using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossStitch.Core.Apps.Messages
{
    public class InstanceCreateRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid ComponentId { get; set; }
        public Guid VersionId { get; set; }

    }
}
