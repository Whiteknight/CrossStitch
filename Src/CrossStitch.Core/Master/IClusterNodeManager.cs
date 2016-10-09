using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Master
{
    public interface IClusterNodeManager : IDisposable
    {
        // Responsible to maintain the current state of the cluster by finding nodes (statically or dynamically)
        // and regular pings to do health assessments
        void Start();
        void Stop();
        IEnumerable<ClusterPeerNode> GetNodesInCluster();
    }
}