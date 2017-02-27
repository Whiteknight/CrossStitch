using System.Collections.Generic;

namespace CrossStitch.Stitch.v1
{
    public interface IToStitchMessageProcessor
    {
        IEnumerable<FromStitchMessage> Process(ToStitchMessage message);
    }
}