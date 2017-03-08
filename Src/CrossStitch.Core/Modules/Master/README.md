## Master Module

The Master Module serves as a brain for a cluster, by allowing commands to affect multiple cluster nodes instead of only the current node. The Master Node will take a few large, complex commands and turn them into a list of smaller atomic commands which can be distributed over the cluster. In this way, we can coordinate large operations.

CrossStitch by default is "leaderless". This means that there isn't a single "Leader" in the cluster which can receive and coordinate complex requests. Each individual node in the cluster can have an active Master module, and each Master module makes decisions based on its understanding of the state of the cluster. If a node in the cluster is not acurately or reliably reporting its status, the Master node might not have up-to-date information about that node and might make some decisions which are incorrect. 

The Master node operates on a best-effort basis. It issues commands to nodes in the cluster and attempts to wait for confirmation of each command. If some of the commands fail, it will report status back to the user so that the user can make a decision about how to proceed (by re-issuing the command, issuing a new catch-up command, or by rolling back the whole thing)

### Examples

I want to deploy an application A. A has two components C1 and C2, with versions VC1 and VC2 respectively. I issue a command to the Master node to deploy N1 versions of Stitch VC1 and N2 versions of Stitch VC2. The master node reviews it's known cluster state information to determine which nodes have space. It then issues the N1+N2 commands to the individual nodes in the cluster. 



