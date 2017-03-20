## Test Projects

These projects are functional CrossStitch-based applications which showcase some of the abilities and serve as ad hoc integration tests for large-scale behaviors.

### ClusterTest

ClusterTest consists of two programs, ClusterTest.Master and ClusterTest.Client. These two applications can be started up in any number or combination. They will form a cluster together and will publish node status between the nodes.

ClusterTest.Master will send periodic ping commands to the other nodes in the cluster, which are expected to respond.

The purpose of this test program is to demonstrate basic connectivity between two nodes, show that they can form a cluster, and show that they can communicate basic command messages and receipts between them.

### PingPong

PingPong consists of three programs. PingPong.Server is a CrossStitch server. PingPong.Ping and PingPong.Pong are both stitches. PingPong.Server starts an instance each of the Ping and Pong nodes. 

PingPong.Ping starts by sending a "ping?" message to the application. PingPong responds to every "ping?" with a "pong!" reply to the sender. PingPong.Ping then responds to every "pong!" with a "ping". The message should go back and forth between the two stitches until the application is stopped.

The application should exit cleanly when a button is pressed, and both stitches should be stopped shortly thereafter.

The purpose of this test is to demonstrate messaging between stitch instances in a single node.

 ### StitchStart

StitchStart.Server is the CrossStitch node. StitchStart.Client is a stitch. 

StitchStart.Server will startup an instance of StitchStart.Client and will send periodic heartbeats. StitchStart.Client will respond to heartbeats with a normal sync and also by logging information about startup parameters. When a key is pressed, the application should cleanly shut down and the stitch instance should also terminate automatically.

This test covers basic stitch startup and communication, including command-line args and basic communication between core and stitch.
