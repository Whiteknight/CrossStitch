namespace CrossStitch.Stitch.Process
{
    public interface IMessageSerializer
    {
        FromStitchMessage DeserializeFromStitchMessage(string message);
        ToStitchMessage DeserializeToStitchMessage(string message);
        string Serialize(ToStitchMessage message);
        string Serialize(FromStitchMessage message);
    }
}
