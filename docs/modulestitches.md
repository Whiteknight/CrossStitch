---
layout: default
title: Stitches Module
---

# Stitches Module

The Stitches module is resonsible for running and managing individual stitches. It is the heart of the CrossStitch system, though it is not a required module. You can run CrossStitch without the Stitches module, if you want the node to focus on other tasks such as routing, monitoring and coordination.

The `StitchesModule` performs basic operations on stitches: Creating, starting, stopping and communications. The Master module is used to route these operation requests between nodes in the cluster. Most requests should be sent to the [Master module](modulemaster.md) first, for routing. The MasterModule will redirect requests to the Stitches module as required.

## Adaptors

See the [adaptors documentation](stitches.md) for more details about different adaptors and how to use them.

## Configuration

The "stitches.json" file holds configuration for the Stitches module. It defines the following keys:

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

### Example Configs

These examples assume the [ProcessV1](adaptorprocessv1.md) protocol. If you are using a different adaptor protocol, you may have different requirements for how to setup your formatting values.

Here is an example of the default config for EXE files. These values are provided by default and do not need to be specified in your files, but this is an example for you to see what it looks like:

    {
      "Extensions": {
        ".exe": {
            "ExecutableFormat": "{DirectoryPath}\\{ExecutableName}",
            "ArgumentsFormat": "{CoreArgs} -- {CustomArgs}"
      }
    }
    

Here is an example config for running a `.js` file as a Stitch, using NodeJs:

    {
      "Extensions": {
        ".js": {
          "ExecutableFormat": "C:\\Program Files\\nodejs\\node.exe",
          "ArgumentsFormat": "{DirectoryPath}\\{ExecutableName} {CoreArgs} -- {CustomArgs}"
        }
      }
    }
