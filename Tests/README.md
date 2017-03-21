## Test Projects

These projects are functional CrossStitch-based applications which showcase some of the abilities and serve as ad hoc integration tests for large-scale behaviors.

### ClusterTest

ClusterTest demonstrates basic connectivity between two nodes to form an ad-hoc cluster using the Zyre backplane. These nodes should form a cluster together and should publish status messages back and forth. This test consists of two components:

1. ClusterTest.Master, a CrossStitch node which periodically sends Ping commands to the client
2. ClusterTest.Client, a CrossStitch node which responds to ping commands with command receipts.

### HttpTest

HttpTest is a proving ground for the NancyFx HTTP API module. It consists of two components:

1. HttpTest.Server, a CrossStitch node which hosts the HTTP server and a stitches module
2. HttpTest.Stitch, a stitch, which can be used to test basic commands and queries on stitches.

### PenPal

PenPal test is a combination of ClusterTest and PingPong test ideas. It establishes two nodes in a cluster, each with a running stitch. These stitches communicate together across the cluster. This test consists of the following components:

1. PenPal.ServerA is the first CrossStitch node, and host of StitchA
2. PenPal.ServerB is the second CrossStitch node, and host of StitchB
3. PenPal.StitchA is a stitch which runs on ServerA. On every heartbeat, it sends a message "A" to all stitches in the "PenPal" application
4. PenPal.StitchB is a stitch which runs on ServerB. When it receives a message "A", it responds to the sender with data "B"

### PingPong

PingPong is a test to show communications between stitches in a single CrossStitch node. It has three components:

1. PingPong.Server, a CrossStitch node which hosts the stitches
2. PingPong.Ping, a stitch which broadcasts the message "ping?" on startup and responds to every "pong!" message with a "ping?" message thereafter
3. PingPong.Pong, a stitch which responds to every "ping?" message with a "pong!" message.

These ping/pong messages should send between the stitches until the server is stopped.

 ### StitchStart

StitchStart demonstrates basic stitch operation including starting, heartbeats and logging. It has two components:

1. StitchStart.Server is a CrossStitch node which hosts the stitch
2. StitchStart.Client is a stitch which responds to heartbeat messages with log messages about its command-line parameters.

When a key is pressed, the application should cleanly shut down and the stitch instance should also terminate automatically.

