# CrossStitch Core

The Core of CrossStitch is comprised of a few bits: The `CrossStitchCore` class which is the top-level and organizes everything else, the `CoreModule` which allows the core to communicate with the message bus, and the `CoreService` which implements some logic specific to the core.

Every `CrossStitchCore` object has an ID and a Name. The Name is friendly and non-unique, and is only included for readability. The node ID should be unique, in the cluster. If it isn't, and CrossStitch cannot enforce this requirement by itself, the status of one node may overwrite the status of other nodes in the cluster.

## Configuration

CrossStitch uses JSON for config files, and each module may have its own configuration as necessay. The Core module and other built-in modules use the file "node.json" to hold config values. The contents of this file are deserialized into a `CrossStitch.Core.NodeConfiguration` object. It has the following keys:

* "NodeId" see below
* "NodeName" see below
* "HeartbeatIntervalMinutes" the number of minutes between heartbeats. Defaults to 1.
* "StitchMonitorIntervalMinutes" How often the core monitors stitch status, in minutes. Defaults to 5.
* "StatusBroadcastIntervalMinutes" how often the core broadcasts its status to the cluster, in minutes. Defaults to 5
* "StateFileFolder" a folder location where node-specific files are stored. If you are running multiple node instances per server, each one must have a unique value for this folder.

## ID

On startup, CrossStitch will attempt to get the ID in the following way:

1. It looks for a "NodeId" value in the `node.json` config file and uses that if it exists
2. It looks for a file "NODEID" in the folder specified by `"StateFileFolder"` config value (defaults to "."), and uses the contents of that file
3. Otherwise it generates a new Guid and writes that guid to a new file called NODEID

If you want to use a particular id for your node, you can either set it up in the `node.json` config file or create a `NODEID` file with the ID value as the only contents of that file. There is no limit on the size of the node ID, but keep in mind that this value is used throughout CrossStitch for various communications, and having very large ID strings is going to cause performance and bandwidth problems. Try to keep them short and meaningful.

## Name

On startup, CrossStitch will attempt to get a friendly name. It uses this procedure:

1. It looks for a "NodeName" value in the `node.json` config file, and uses that if it exists
2. It looks for a file "NODENAME" in the folder specified by `"StateFileFolder"` config value (defaults to "."), and uses the contents of that file
3. Otherwise it uses the NodeID as the name, and writes that value to a new file called NODENAME

The process is nearly identical to NodeId.