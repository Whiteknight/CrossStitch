using System.Collections.Generic;

namespace CrossStitch.Stitch.v1
{
    public interface IMessageProcessor
    {
        IEnumerable<FromStitchMessage> Process(ToStitchMessage message);
    }
}