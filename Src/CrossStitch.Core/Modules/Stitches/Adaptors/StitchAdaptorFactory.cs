using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1;
using System;
using CrossStitch.Core.Modules.Stitches.Adaptors.Process;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        private readonly string _nodeId;
        private readonly StitchesConfiguration _configuration;
        private readonly StitchFileSystem _fileSystem;

        public StitchAdaptorFactory(string nodeId, StitchesConfiguration configuration, StitchFileSystem fileSystem)
        {
            _nodeId = nodeId;
            _configuration = configuration;
            _fileSystem = fileSystem;
        }

        public IStitchAdaptor Create(PackageFile packageFile, StitchInstance stitchInstance)
        {
            var context = new CoreStitchContext(stitchInstance.Id);
            switch (packageFile.Adaptor.Type)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case AdaptorType.ProcessV1:
                    var pv1args = new ProcessParameters(_configuration, _fileSystem, stitchInstance, packageFile);
                    return new ProcessStitchAdaptor(_nodeId, _configuration, stitchInstance, context, pv1args);
                case AdaptorType.BuildInClassV1:
                    return new BuiltInClassV1StitchAdaptor(packageFile, stitchInstance, context);
            }

            throw new Exception("Run mode not supported");
        }
    }
}