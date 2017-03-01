using System.Collections.Generic;

namespace CrossStitch.Stitch.V1.Stitch
{
    public interface IToStitchMessageProcessor
    {
        IEnumerable<FromStitchMessage> Process(ToStitchMessage message);
    }
}