namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public interface IStitchRequestHandler
    {
        bool StartInstance(string instanceId);
        bool StopInstance(string instanceId);
        bool RemoveInstance(string instanceId);
    }
}