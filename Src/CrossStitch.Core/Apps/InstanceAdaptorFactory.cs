using System;

namespace CrossStitch.Core.Apps
{
    public class InstanceAdaptorFactory
    {
        public IAppAdaptor Create(ComponentInstance instance)
        {
            switch (instance.RunMode)
            {
                case InstanceRunModeType.AppDomain:
                    return new AppDomainAppAdaptor(instance);
                case InstanceRunModeType.Process:
                    return new ProcessAppAdaptor(instance);
            }

            throw new Exception("Run mode not supported");
        }
    }
}