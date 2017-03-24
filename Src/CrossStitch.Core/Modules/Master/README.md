## Master Module

The Master Module serves as a brain or router for a cluster, keeping track of nodes on the network, and allowing routing and dispatch of events and commands between nodes. The Master module serves several purposes, which seem distinct at first glance but are all related to the same underlying requirement: Keeping track of cluster state.

1. Publishing the state of the current running node to the cluster
2. Receiving and storing state published by other nodes in the cluster
3. Responding to requests for node state (from, for example, the HTTP API)
4. Routing messages to individual stitches, by looking up what node the stitch lives on and sending the message to that node
5. Breaking up complex commands to commands for individual nodes, and dispatching those individual commands to nodes in the cluster.

### Messaging

The Master Module subscribes to the following types of events:

1. Cluster status updates from the Backplane
2. Node status updates from the Backplane
3. Stitch status updates from the Stitches module

### Command Behavior

The Master module receives `CommandRequest` request messages and is expected to return `CommandResponse` messages. For each `CommandRequest` the Master module needs to consult the cluster state to figure out how the command should be routed. For the example of a command to "Start Stitch Instance", the Master mode will determine which node the stitch currently lives on. If the stitch instance is local, the Master module will generate an `InstanceRequest` message, send that to the StitchesModule, receive the `InstanceResponse` message and convert that to a `CommandResponse`.

If the Stitch instance is remote, the Master module will generate a `Job` object, dispatch the command over the Backplane with requested receipt, and return a `ClusterResponse` message with the `Job` ID. The user can then lookup the status of the `Job` object later to see whether it succeeded or failed.

### Leaders and Leaderlessness

The Master node is responsible for keeping track of the state of the cluster. The default Master module implementation is "leaderless". This means that the default Master module does not elect a leader for the cluster, and therefore does not maintain a single source of truth. Every node can be asked about current cluster status, and every node can individually give back a different opinion of it, depending on what it can see at that time.

Alternate Master module implementations may implement leadership ideas through algorithms such as RAFT or PAXOS, and route certain types of requests to the leader instead of handling them immediately. What's the difference?

In a leaderless system, when a complex request comes in, the current node uses it's current information about the cluster to perform the request immediately. The currently-known information about the cluster may be incomplete, leading to inconsistent results. In a leadership system, the complex request is either routed to the leader to perform, or the current cluster state is replicated from the "source of truth" on the leader to make sure that the worker has an official, up-to-date version of the cluster state. This kind of system avoids inconsistency, but is significantly more complex to implement and may suffer from other problems such as "split brain" cluster partitions.

CrossStitch does not take a position on whether leaderless or leader-based systems are superior to the other. The default implementation of the Master module is leaderless only for reasons of simplicity.

Currently there is no non-default implementation of the Master module. A version which supports leadership concepts using an algorithm such as RAFT or PAXOS is certainly possible in the CrossStitch architecture and even desireable.
