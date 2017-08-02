namespace CrossStitch.Stitch.Process.Stitch
{
    public interface IToStitchMessageProcessor
    {
        bool Process(ToStitchMessage message);
    }
}