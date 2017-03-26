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

        public StitchAdaptorFactory(StitchesConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IStitchAdaptor Create(StitchInstance stitchInstance)
        {
            var context = new CoreStitchContext(stitchInstance.Id);
            switch (stitchInstance.Adaptor.Type)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case AdaptorType.ProcessV1:
                    return new ProcessV1StitchAdaptor(_configuration, stitchInstance, context);
                case AdaptorType.BuildInClassV1:
                    return new BuiltInClassV1StitchAdaptor(stitchInstance, context);
            }

            throw new Exception("Run mode not supported");
        }
    }
}