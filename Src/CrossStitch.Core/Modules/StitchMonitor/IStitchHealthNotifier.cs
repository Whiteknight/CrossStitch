namespace CrossStitch.Core.Modules.StitchMonitor
{
    public interface IStitchHealthNotifier
    {
        void NotifyUnhealthy(string instanceId);
        void NotifyReturnToHealth(string instanceId);
    }
}