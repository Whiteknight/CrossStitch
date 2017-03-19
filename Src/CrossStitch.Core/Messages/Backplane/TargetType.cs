namespace CrossStitch.Core.Messages.Backplane
{
    public enum TargetType
    {
        // Address a message to a node or nodes
        Cluster,
        Node,
        Zone

        // Messages which are being sent directly to a stitch instance or a stitch group will have
        // TargetType.Node or TargetType.Cluster, but the payload object will contain routing information
        // which the Master module will decipher and dispatch as required
    }
}