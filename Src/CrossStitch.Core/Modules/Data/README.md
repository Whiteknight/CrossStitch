## Data Module

The data module is responsible for storing persistant state of the CrossStitch node. It holds information about entities such as the state of the node, Applications, Stitch Instances, and a few other things as required by the core and various modules.

The Data module should have a name "Data". If a data module isn't provided by the startup application, the Core may create one.

The `DataModule` receives all request on a single thread, to simplify storage scenarios for simple storage engines. From that single thread, the `IDataStorage` engine handles all requests.

### IDataStorage

The Data module receives requests from the message bus and dispatches them to the `IDataStorage` instance. `IDataStorage` is fully pluggable and customizable, and can target any desired storage backend. The two built-in options are `InMemoryDataStorage` and `FolderDataStorage`

#### InMemoryDataStorage

The `InMemoryDataStorage` engine stores data in memory and does not persist to disk. Every time the node restarts, this storage will be empty. This is useful for testing scenarios and for scenarios where the node is self-assembling.

#### FolderDataStorage

The `FolderDataStorage` engine stores each record as a json-formatted file in a directory structure. As part of configuration, you must provide a directory path to store the files.

All data is stored in files named [DataPath]\[TypeName]\[Id].json

### Overriding and Extending

There are two ways to change the behavior of the DataModule. 

1. Create a new `IModule` with Name="Data" and register it with the core. This module should respond to `DataRequest<T>` messages and respond with appropriate `DataResponse<T>` messages, at least.
2. Create a new `IDataStorage` (recommended). Load this into the existing `DataModule` and implement the necessary methods to respond to requests.
