using CrossStitch.Core.Data.Entities;
using CrossStitch.Stitch;
using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class StitchAdaptorFactory
    {
        private readonly IRunningNodeContext _nodeContext;

        public StitchAdaptorFactory(IRunningNodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public IStitchAdaptor Create(Instance instance)
        {
            switch (instance.Adaptor.RunMode)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case InstanceRunModeType.V1Process:
                    return new V1ProcessStitchAdaptor(instance, _nodeContext);
            }

            throw new Exception("Run mode not supported");
        }
    }
}