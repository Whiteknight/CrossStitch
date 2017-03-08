## Request Coordinator Module

Requests to change state can come from many sources: an HTTP API, various modules inside the CrossStitch node, and other nodes from across the CrossStitch cluster. These incoming requests typically need some level of coordination: Changing the state of one or more Stitches, updating stored data records, triggering logging and internal events, and other examples.

The Request Coordinator module listens to change events and coordinates the actions required by the various other modules and components of the system.

This is an internal module and does not need to be explicitly created or managed by the user. 