## NancyFX Http Module

The Nancy HTTP module implements a RESTful HTTP API for CrossStitch. This API allows a remote user to issue queries to the node to get status information, and also issue commands to the node to change state. 

Without the HTTP module and without another commanding module, the CrossStitch core node will not be able to change state from its initial startup configuration, and will not be able to easily report status to the user.