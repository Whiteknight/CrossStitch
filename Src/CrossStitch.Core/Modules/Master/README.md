## Master Module

The Master Module serves as a brain for a cluster, keeping track of nodes on the network, and allowing routing and dispatch of events and commands between nodes. The Master module serves several purposes, which seem distinct but have several shared requirements which prevent them from being cleanly separated (at this time).

1. Keeping track of the state of nodes in the cluster
2. Publishing the state of the current running node
3. Responding to requests for node state
4. Routing messages to individual stitches, by looking up what node the stitch lives on and sending the message to that node
5. Breaking up complex commands to commands for individual nodes, and dispatching those individual commands to nodes in the cluster.

### Leaders and Leaderlessness

CrossStitch by default is "leaderless". This means that there isn't a single "Leader" in the cluster which is guaranteed to receive and coordinate complex requests. There is no single, default, source of truth which all other nodes replicate. Each individual node in the cluster can have an active Master module, and each Master module makes decisions based on its understanding of the state of the cluster at that time. If a node in the cluster is not accurately or reliably reporting its status, the Master node might not have up-to-date information about that node and might make some decisions which are incorrect.

Because the cluster is leaderless by default (alternate implementations of the Master node or other nodes can change that behavior, of course), Conflicting commands sent at the same time to different nodes can have conflicted outcomes. As a matter of implementation and cultural best practice, consider designating one node in the cluster as the "preferred" Master, and asking all CrossStitch users very nicely to direct commands to that one node only. Or, you can use all the nodes in the cluster for load balancing, but ask people to coordinate among themselves before issuing commands.

It is possible to override the default `MasterModule` instance with a version which does implement a strong leader election algorithm such as RAFT or PAXOS, and to route all requests to the master and replicate state from the master only. This is possible because all of these features are encapsulated within a single module. The current default implementation of Master is too simple for this, and can be used for cases where a strong leader and the guarantees which come along with it are not necessary.




