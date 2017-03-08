namespace CrossStitch.Backplane.Zyre.Networking
{
    public enum TargetType
    {
        // TODO: We need to be able to address messages to the following destinations:
        // 1) All stitches under a given application
        // 2) All stitches under a given application.component
        // 3) All stitches under a given application.component.version
        Cluster,
        Node,
        AppInstance,
        Zone,
        Local
    }
}