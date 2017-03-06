using System;

namespace CrossStitch.Stitch.Events
{
    public static class EventHandlerExtensions
    {
        public static void Raise<TArgs>(this EventHandler<TArgs> handler, object sender, TArgs args)
            where TArgs : EventArgs
        {
            handler?.Invoke(sender, args);
        }

        public static void Raise<TPayload>(this EventHandler<PayloadEventArgs<TPayload>> handler, object sender, string command, TPayload payload)
        {
            handler?.Invoke(sender, new PayloadEventArgs<TPayload>(command, payload));
        }
    }
}
