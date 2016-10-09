using System;
using CrossStitch.Core.Messaging.Threading;

namespace CrossStitch.Core.Messaging.PubSub
{
    public class PublishEventThreadAction<TPayload> : IThreadAction
    {
        private readonly Action<TPayload> _act;
        private readonly TPayload _payload;

        public PublishEventThreadAction(Action<TPayload> act, TPayload payload)
        {
            _act = act;
            _payload = payload;
        }

        public void Execute(MessageHandlerThreadContext threadContext)
        {
            _act(_payload);
        }
    }
}