namespace CrossStitch.Core.Messaging.Threading
{
    public interface IThreadAction
    {
        void Execute(MessageHandlerThreadContext threadContext);
    }
}