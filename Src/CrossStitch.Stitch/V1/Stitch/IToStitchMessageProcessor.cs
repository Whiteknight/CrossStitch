namespace CrossStitch.Stitch.V1.Stitch
{
    public interface IToStitchMessageProcessor
    {
        bool Process(ToStitchMessage message);
    }
}