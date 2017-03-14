## Stitches Module

The Stitches module is resonsible for running and managing individual stitch processes. The Stitches module is the heart of CrossStitch though it is not a required module. It is possible to run a CrossStitch node without the Stitches module running. In this case the CrossStitch node can still perform other tasks depending on which modules are installed, such as responding to requests over an HTTP API, keeping track of cluster state, routing and coordination tasks between nodes, etc. However, most nodes in your cluster will probably want this module to be running.

The `StitchesModule` performs basic operations on stitches: Creating, starting, and stopping. The Master module is used to route these operation requests between nodes in the cluster, and the Request Coordinator module is used to coordinate most requests with data storage and other concerns. Most requests should be sent to the Master or Request Coordinator modules, which will redirect to the Stitches module as necessary

### Adaptors

An Adaptor is a type of class which wraps the running stitch, provides control over it, and enforces a communications protocol. When a stitch is created, the type of adaptor to use should be specified.

#### V1Process Adaptor

This adaptor runs stitches as independent processes. Arguments are passed on the commandline and communication happens over standard input and output streams. Because of this, V1Process is fully language agnostic. Any programming language, libraries or tools can be used, so long as they conform to the communications protocol.


Command-Line arguments are passed to the program using `Key=Value` format. Core arguments are separated from custom arguments with the `--` symbol. One of the arguments passed is `corepid` which holds the OS PID value of the CrossStitch process. It is the responsibility of the Stitch program to monitor this PID and exit when the core has exited. The V1Process adaptor will also pass arguments for the stitch to know the applicationID, component, version and group name to which the process belongs. These pieces of information can be used for sending data messages to other stitches in the group, or can influence how the stitch behaves if a single executable defines multiple different behaviors.

	corepid=5 ... -- ...custom arguments ...

V1Process communicates using a protocol of JSON over the STDIN and STDOUT streams. This protocol was inspired by a similar protocol used by Apache Storm, though the two are not compatible.

See the Classes defined in the V1 namespace of the `CrossStitch.Stitch.dll` project for more details about this protocol.

