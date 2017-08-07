using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1;
using System;
using CrossStitch.Core.Modules.Stitches.Adaptors.Process;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        private readonly CrossStitchCore _core;
        private readonly StitchesConfiguration _configuration;
        private readonly StitchFileSystem _fileSystem;
        private readonly IModuleLog _log;

        public StitchAdaptorFactory(CrossStitchCore core, StitchesConfiguration configuration, StitchFileSystem fileSystem, IModuleLog log)
        {
            _core = core;
            _configuration = configuration;
            _fileSystem = fileSystem;
            _log = log;
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
                    return new ProcessStitchAdaptor(_core, _configuration, stitchInstance, context, pv1args, _log);
                case AdaptorType.BuildInClassV1:
                    return new BuiltInClassV1StitchAdaptor(packageFile, context, _log);
            }

            throw new Exception("Run mode not supported");
        }
    }
}