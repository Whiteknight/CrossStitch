namespace CrossStitch.Core.Models
{
    public enum InstanceStateType
    {
        // The stitch has started, but hasn't yet checked in
        Started,

        // The stitch is running
        Running,

        // The stitch could not be started
        Error,

        // The stitch has been sent the exit command
        Stopping,

        // The stitch has been stopped
        Stopped,

        // The stitch should be running, but it cannot be found.
        Missing
    }
}