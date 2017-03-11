using CrossStitch.Core;
using CrossStitch.Core.Utility;

namespace CrossStitch.Backplane.Zyre
{
    public sealed class ZyreBackplaneFactory : IFactory<IClusterBackplane, CrossStitchCore>
    {
        private readonly BackplaneConfiguration _configuration;

        public ZyreBackplaneFactory(BackplaneConfiguration configuration = null)
        {
            _configuration = configuration;
        }

        public IClusterBackplane Create(CrossStitchCore core)
        {
            return new ZyreBackplane(core, _configuration);
        }
    }
}