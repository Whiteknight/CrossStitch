using CrossStitch.Core.Models;
using System;
using CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1;
using CrossStitch.Stitch.ProcessV1.Core;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        public IStitchAdaptor Create(StitchInstance stitchInstance)
        {
            var context = new CoreStitchContext(stitchInstance.Id);
            switch (stitchInstance.Adaptor.Type)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case AdaptorType.ProcessV1:
                    return new ProcessV1StitchAdaptor(stitchInstance, context);
            }

            throw new Exception("Run mode not supported");
        }
    }
}