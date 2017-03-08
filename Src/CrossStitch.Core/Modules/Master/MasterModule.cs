using CrossStitch.Core.Node;

namespace CrossStitch.Core.Modules.Master
{
    // The Master module coordinates multipart-commands across the cluster.
    public class MasterModule : IModule
    {
        private CrossStitchCore _runningNode;
        private readonly IClusterNodeManager _nodeManager;

        public MasterModule(IClusterNodeManager nodeManager)
        {
            _nodeManager = nodeManager;
        }

        // TODO: We need some kind of scoring metric for a node to report, which will take into 
        // account the number of processor cores and available RAM, and reduce by the number of
        // running stitches, so we can know which nodes to deploy stitches to.

        // TODO: We need to be storing, through the Data Module, state instance of all nodes in the
        // cluster, including the current running node. 

        /* Commands to support:
         * 1) Create N instances of a Stitch version, with automatic balancing across the cluster to nodes with space
         * 2) Move a stitch instance from current Node to specified remote Node
         * 3) Move a stitch instance from Specified remote node to current node
         * 4) Rebalance instances, by moving stitch instances from an overloaded node to an underloaded node
         * 5) Shutdown stitch instances of a particular version, if A instances are needed but B are running in the cluster and B>A
         * 6) Shutdown all stitch instances of any version besides the current version V, on all nodes.
         * 7) Create A-B stitch instances if A instances are needed but B are running in the cluster and A>B
         * 8) Deploy an application, by Deploying Ni instances of each Stitch/Component version I in an application manifest
         *      The manifest will include specific versions for each component and a number of instances to run for each component version.
         *      
         * Whether we want to represent these things by command objects or by some kind of parsible command script is to be determined.
         */

        // TODO: When we convert an input command to a stream of output commands, we will need some
        // kind of a Job object that we can use to keep track of state. The status of the Job can be
        // queried externally, and the job can be polled regularly to find jobs which are running 
        // very late and need to be alerted about.
        // TODO: A Job should have an ability to be rolled-back, by issuing a sequence of inverse 
        // commands.

        // TODO: Method to lookup NodeId by NetworkNodeId and vice-versa

        //private static bool IsMessageAddressedToAppInstance(MessageEnvelope arg)
        //{
        //    return arg.Header.ToType == TargetType.AppInstance;
        //}

        //private void ResolveAppInstanceNodeIdAndSend(MessageEnvelope obj)
        //{
        //    throw new NotImplementedException();
        //    // TODO: Resolve the NodeId for the message and publish again.
        //}

        public string Name => "Master";

        public void Start(CrossStitchCore context)
        {
            _runningNode = context;
            _nodeManager.Start();

            //messageBus.Subscribe<MessageEnvelope>(s => s
            //    .WithChannelName(MessageEnvelope.SendEventName)
            //    .Invoke(ResolveAppInstanceNodeIdAndSend)
            //    .OnWorkerThread()
            //    .WithFilter(IsMessageAddressedToAppInstance)
            //);
        }

        public void Stop()
        {
            if (_runningNode == null)
                return;
            _runningNode = null;
            _nodeManager.Stop();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}