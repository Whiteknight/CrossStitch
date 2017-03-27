# Stitches Module

The Stitches module is resonsible for running and managing individual stitches. The Stitches module is the heart of CrossStitch though it is not a required module. It is possible to run a CrossStitch node without the Stitches module running. In this case the CrossStitch node can still perform other tasks depending on which modules are installed, such as responding to requests over an HTTP API, keeping track of cluster state, routing and coordination tasks between nodes, etc. However, most nodes in your cluster will probably want this module to be running.

The `StitchesModule` performs basic operations on stitches: Creating, starting, and stopping. The Master module is used to route these operation requests between nodes in the cluster. Most requests should be sent to the MasterModule first, for routing. The MasterModule will redirect requests to the Stitches module as required.

## Adaptors

See the [adaptors documentation](stitches.md) for more details about different adaptors and how to use them.

## Configuration

The "stitches.json" file holds configuration for the StitchesModule. It defines the following keys:

* "DataBasePath" a folder where stitches can store data. Every stitch instance gets a subfolder here to use for storing data
* "AppLibraryBasePath" A folder where .zip files are stored for each stitch type, so they can be quickly instantiated
* "RunningAppBasePath" A folder where the unzipped stitch files are stored, so the stitches can be executed
* "Extensions" holds information for executables which need special handling. Each entry in this object is a file extension which points to some formatting details:
** "ExecutableFormat"
** "ArgumentsFormat"

The two "Format" values hold strings which are used to format both the name of the executable and the arguments to pass to it. 

### ExecutableFormat

There are two variables, `{DirectoryPath}` is the name of the directory where the stitch executable lives, and `{ExecutableName}` is the name of the executable file. The default value for ExecutableFormat is `"{DirectoryPath}\{ExecutableName}"`

If you are writing a Stitch in a language which is not compiled, the executable should probably point to the VM or interpreter for your language, and the script file should be handled in the arguments format. For example, for the ".js" extension, you can set ExecutableFormat to something like "\Path\To\node.exe".

### ArgumentsFormat

This has four variables, `{DirectoryPath}`, `{ExecutableName}` are the same as above, `{CoreArgs}` are arguments setup by CrossStitch and `{CustomArgs}` are arguments for the application that the user specifies. The default value for this is `"{CoreArgs} -- {CustomArgs}"`. If you are using a language which is not compiled and you have the VM/interpreter spcified in ExecutableFormat, you can use something like this for ArgumentsFormat: "{DirectoryPath}\{ExecutableName} {CoreArgs} -- {CustomArgs}". Consult the documentation for your target runtime for more information about how to setup your arguments.


