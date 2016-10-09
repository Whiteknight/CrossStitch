namespace CrossStitch.Core.Messaging.Threading
{
    public enum DispatchThreadType
    {
        NoPreference,
        Immediate,
        SpecificThread,
        AnyWorkerThread
    }
}