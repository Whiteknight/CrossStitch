﻿using System;
using CrossStitch.Core.Models;

namespace CrossStitch.Core.Messages.Stitches
{
    public class InstanceInformation
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }

        public Guid ComponentId { get; set; }
        public string ComponentName { get; set; }

        public Guid VersionId { get; set; }
        public string Version { get; set; }

        public InstanceStateType State { get; set; }
    }
}