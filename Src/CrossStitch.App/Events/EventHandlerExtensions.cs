using System;

namespace CrossStitch.App.Events
{
    public static class EventHandlerExtensions
    {
        public static void Raise<TArgs>(this EventHandler<TArgs> handler, object sender, TArgs args)
            where TArgs : EventArgs
        {
            if (handler != null)
                handler(sender, args);
        }

        public static void Raise<TPayload>(this EventHandler<PayloadEventArgs<TPayload>> handler, object sender, string command, TPayload payload)
        {
            if (handler != null)
                handler(sender, new PayloadEventArgs<TPayload>(command, payload));
        }
    }
}
