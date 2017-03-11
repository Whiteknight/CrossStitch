using CrossStitch.Core.Models;
using CrossStitch.Stitch.V1.Core;
using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        public IStitchAdaptor Create(StitchInstance stitchInstance)
        {
            var context = new CoreStitchContext(stitchInstance.Id);
            switch (stitchInstance.Adaptor.RunMode)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case InstanceRunModeType.V1Process:
                    return new V1ProcessStitchAdaptor(stitchInstance, context);
            }

            throw new Exception("Run mode not supported");
        }
    }
}