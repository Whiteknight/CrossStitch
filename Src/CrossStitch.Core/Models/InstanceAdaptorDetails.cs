using System.Collections.Generic;
using CrossStitch.Stitch.Process;

namespace CrossStitch.Core.Models
{
    public class InstanceAdaptorDetails
    {
        public AdaptorType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public MessageChannelType Channel { get; set; }
        public MessageSerializerType Serializer { get; set; }

        private bool? _requiresPackageUnzip;

        public bool RequiresPackageUnzip
        {
            get
            {
                if (_requiresPackageUnzip.HasValue)
                    return _requiresPackageUnzip.Value;
                return Type == AdaptorType.ProcessV1;
            }
            set { _requiresPackageUnzip = value; }
        }
    }
}