## Zyre Backplane Module

The Zyre backplane module uses the Zyre project to implement a clustering algorithm for CrossStitch nodes across a network. Zyre handles detection of peer nodes and assembling the nodes into a cluster. It also implements communication channels and zones for heterogenous clusters.

Without a Backplane module installed, the CrossStitch application will only run as an independent single node and will not coordinate with other nodes in the network.