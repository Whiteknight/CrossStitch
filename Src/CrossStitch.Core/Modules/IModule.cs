using System;
using CrossStitch.Core.Node;

namespace CrossStitch.Core.Modules
{
    public interface IModule : IDisposable
    {
        // A top-level module corresponding to the roles and responsibilities of the local node
        string Name { get; }
        void Start(CrossStitchCore context);
        void Stop();
    }
}