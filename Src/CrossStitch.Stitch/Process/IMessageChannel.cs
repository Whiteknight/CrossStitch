namespace CrossStitch.Stitch.Process
{
    public interface IMessageChannel
    {
        string ReadMessage();
        void Send(string message);
    }
}