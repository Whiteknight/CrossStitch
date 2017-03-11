using CrossStitch.Core;
using CrossStitch.Core.Utility;

namespace CrossStitch.Backplane.Zyre
{
    public sealed class ZyreBackplaneFactory : IFactory<IClusterBackplane, CrossStitchCore>
    {
        public IClusterBackplane Create(CrossStitchCore core)
        {
            return new ZyreBackplane(core);
        }
    }
}