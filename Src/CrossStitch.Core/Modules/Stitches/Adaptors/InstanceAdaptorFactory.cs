using System;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Networking;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public class InstanceAdaptorFactory
    {
        private readonly INetwork _network;
        private readonly string _nodeName;

        public InstanceAdaptorFactory(INetwork network, string nodeName)
        {
            _network = network;
            _nodeName = nodeName;
        }

        public IAppAdaptor Create(Instance instance)
        {
            switch (instance.Adaptor.RunMode)
            {
                //case InstanceRunModeType.AppDomain:
                //    return new AppDomainAppAdaptor(instance, _network);
                case InstanceRunModeType.V1Process:
                    return new V1ProcessStitchAdaptor(instance, _nodeName);
            }

            throw new Exception("Run mode not supported");
        }
    }
}