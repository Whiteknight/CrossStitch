using System;
using CrossStitch.App.Networking;

namespace CrossStitch.Core.Apps
{
    public class InstanceAdaptorFactory
    {
        private readonly INetwork _network;

        public InstanceAdaptorFactory(INetwork network)
        {
            _network = network;
        }

        public IAppAdaptor Create(ComponentInstance instance)
        {
            switch (instance.RunMode)
            {
                case InstanceRunModeType.AppDomain:
                    return new AppDomainAppAdaptor(instance, _network);
                case InstanceRunModeType.Process:
                    return new ProcessAppAdaptor(instance, _network);
            }

            throw new Exception("Run mode not supported");
        }
    }
}