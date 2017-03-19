namespace CrossStitch.Stitch.ProcessV1.Stitch
{
    public interface IToStitchMessageProcessor
    {
        bool Process(ToStitchMessage message);
    }
}