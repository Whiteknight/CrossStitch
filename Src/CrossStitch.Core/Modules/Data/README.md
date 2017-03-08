﻿## Data Module

The data module is responsible for storing persistant state of the CrossStitch node. It holds information about entities such as the state of the node, Applications, Stitch Instances, and a few other things as required by the core and various modules.

The Data module should have a name "Data". If a data module isn't provided by the startup application, the Core may create one.

The `DataModule` receives all request on a single thread, to simplify storage scenarios for simple storage engines. From that single thread, the `IDataStorage` engine handles all requests.

### InMemoryDataStorage

The `InMemoryDataStorage` engine stores data in memory and does not persist to disk. Every time the node restarts, this storage will be empty. This is useful for testing scenarios and for scenarios where the node is self-assembling.

### FolderDataStorage

The `FolderDataStorage` engine stores each record as a json-formatted file in a directory structure. As part of configuration, you must provide a directory path to store the files.