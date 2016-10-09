using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging
{
    public class PublishOptions
    {
        public PublishOptions()
        {
            WaitTimeoutMs = 1000;
            ThreadId = 0;
            DispatchType = DispatchThreadType.Immediate;
        }

        public DispatchThreadType DispatchType { get; set; }
        public int ThreadId { get; set; }
        public int WaitTimeoutMs { get; set; }

        public static PublishOptions Default
        {
            get { return new PublishOptions(); }
        }

        public static PublishOptions SpecificThread(int threadId)
        {
            return new PublishOptions {
                DispatchType = DispatchThreadType.SpecificThread,
                ThreadId = threadId
            };
        }
    }
}