﻿namespace CrossStitch.Core.Timer
{
    public class MessageTimerEvent
    {
        public const string EventName = "Tick";
        public MessageTimerEvent(long id)
        {
            Id = id;
        }

        public long Id { get; private set; }
    }
}