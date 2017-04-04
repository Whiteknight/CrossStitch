using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1;
using CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1;
using CrossStitch.Stitch.ProcessV1.Core;
using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        private readonly StitchesConfiguration _configuration;
        private readonly StitchFileSystem _fileSystem;

        public StitchAdaptorFactory(StitchesConfiguration configuration, StitchFileSystem fileSystem)
        {
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
                    var pv1args = new ProcessV1Parameters(_configuration, _fileSystem, stitchInstance, packageFile.Adaptor.Parameters);
                    return new ProcessV1StitchAdaptor(_configuration, stitchInstance, context, pv1args);
                case AdaptorType.BuildInClassV1:
                    return new BuiltInClassV1StitchAdaptor(packageFile, stitchInstance, context);
            }

            throw new Exception("Run mode not supported");
        }
    }
}