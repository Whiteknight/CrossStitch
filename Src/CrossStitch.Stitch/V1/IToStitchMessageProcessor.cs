using System.Collections.Generic;

namespace CrossStitch.Stitch.V1
{
    public interface IToStitchMessageProcessor
    {
        IEnumerable<FromStitchMessage> Process(ToStitchMessage message);
    }
}