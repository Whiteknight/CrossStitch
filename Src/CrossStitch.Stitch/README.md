# CrossStitch.Stitch

`CrossStitch.Stitch` is the C# reference library for creating Stitch programs in C# to host under CrossStitch. The protocol for creating a Stitch is relatively simple and straight-forward, and this library is **not required** to create a functioning Stitch. Because the Stitch protocol is based only on OS-level features (command-line arguments and STDIN/STDOUT streams), you can create your own interface or use a different programming language entirely with no side effects or repercussions.

CrossStitch intends to support multiple adaptor types and protocols to communicate with stitches. The default and most simple is called `ProcessV1`. This protocol is inspired by several bits of prior art, though the readline-based protocol is mostly based on Apache Storm. The `ProcessV1` protocol is not directly compatible with Apache Storm, however.

Examples in this document will focus on the `ProcessV1` protocol for ease of quick-start development.

## Quick Example

    using CrossStitch.Stitch.ProcessV1.Stitch;
    class Program
    {
        private static StitchMessageManager _manager;
        static void Main(string[] args)
        {
            _manager = new StitchMessageManager(args);
            _manager.Start();

            // Start worker threads or tasks here

            while (true)
            {
                var msg = _manager.GetNextMessage();
                if (msg == null)
                    continue;

                // Do some kind of processing on the message here

                // Acknowledge that the message was received and handled successfully
                _manager.AckMessage(msg.Id);
            }
        }
    }

`StitchMessageManager` implements the communication protocol. Using the manager you can get incoming data and event messages from the CrossStitch core and you can send back acknowledgements, logs and data.

## Messages

The Core generates messages of type `ToStitchMessage` to send to the Stitch, and the Stitch sends messages of type `FromStitchMessage` back to the core. This messaging system is asynchronous and bi-directional. The Stitch does not need to wait for a message from the Core to respond, or vice-versa. 

`ToStitchMessage` contains a `ChannelName` property which tells the name of the channel. Channel names which start with an underscore (`'_'`) are typically reserved for special purpose in the CrossStitch protocol. `ToStitchMessage` also contains information about the message sender (StitchId, NodeId), and the `Data` of the message. `Data` is a string whose format is left to the discretion of the Stitch developer. Suggested formats are JSON (for structured data) and Base64-encoded binary (for binary or unreadable data). Despite these suggestions, CrossStitch does not inspect, validate or care about what you put in your `Data`. Leave it null for all we care. 

`FromStitchMessage` contains a `ChannelName` with the same restrictions as above, `Data`, and information about the recipient (StitchId or Stitch Group Name). Special response channels start with an underscore (`'_'`) and are routed automatically. `FromStitchMessage` type has a number of factory methods on it to create messages of the correct type and routing information.

## Exiting, Graceful and Otherwise

When the CrossStitch application exits expectedly, it will attempt to stop Stitches gracefully by sending an `"_exit"` message to the Stitch. The default behaviour in response to an exit message is for the stitch to immediately terminate, although a more graceful method of termination is possible.

The CrossStitch application might not exit in a graceful manner, however. The application could be force-stopped by the user or it could crash for a number of reasons. In these cases, the Core application will not be able to send graceful exit messages to the Stitch. Because Stitches are separate processes and because Windows does not automatically kill child processes when the parent exits, CrossStitch stitches are expected to check for the existance of the Core process and terminate themselves if the Core unexpectedly disappears. The `CrossStitch.Stitch` library implements this behavior already. Here's how:

1. When the Stitch is started, the Core passes a commandline parameter `"CorePID=123"` to the stitch. In the code example above, the commandline `args` are passed to the `StitchMessageManager`, where this value can be parsed out.
2. Periodically, the `StitchMessageManager` will use the Core PID to check if the application is still running or has exited.
3. If the Core process has exited, `StitchMessageManager` will queue up an `"_exit"` message
4. The default behavior for `StitchMessageManager` is to immediately terminate the application in response to an `"_exit"` message in the method `GetNextMessage()`.

If you are developing a Stitch without using the `CrossStitch.Stitch` library, such as writing a Stitch in another language, you can follow this algorithm or you can invent your own. For example, the Stitch expects to receive a `"_heartbeat"` message from the core periodically. If a heartbeat message has not been received after a long time, you can detect that as an error situation and exit. This may be easier than polling the Core PID in some situations. See the next section on heartbeats for more details.

If you process one data message from the Core at a time, having the Stitch terminate in the call to `GetNextMessage()` will allow you to save all the data from processing the previous message before an exit could be triggered. However if you are doing some concurrent handling or have worker threads busy with other tasks, you might prefer to detect the exit message and bring things down to a graceful stop. You can do that like this:

    // Set this flag to get the exit message instead of exiting immediately
    _manager.ReceiveExitMessages = true;

    var msg = _manager.GetNextMessage();
    if (msg.IsExitMessage())
    {
        // Graceful shutdown here.
    }

A stitch may continue running after the CrossStitch core process has terminated, if there is more work to be done or if there are tasks which are partially done and need to be completed first. There are several caveats in these scenarios:

1. The Core will not be available for things like logging, heartbeats or sending Stitch-to-Stitch messages.
2. The Stitch cannot be attached to a new Core, if the service is restarted later. The new CrossStitch process may attempt to start new instances of the same Stitch ID, which may conflict with resources in use by the previous instance.
3. The Stitch process will be running headless and will be difficult to monitor or manage without the Core running. You won't even know you have these zombie processes running until you open your task manager and look for them. Too many stop/restart cycles creating zombie processes can choke the life out of your server.

CrossStitch does allow each stitch to manage its own shutdown gracefully, but it is not a time to dilly-dally. Finish whatever work is necessary and terminate the application as quickly as possible to avoid complications.

## Heartbeats and Health Status

At a configurable interval, 1 minute by default, the CrossStitch Core will send a `"_heartbeat"` message to every Stitch. The Stitch is expected to respond with a `"_sync"` message, to let the Core know that the Stitch is healthy and responsive. Responding to all heartbeats will have the Stitch status reported as "Green". Missing a few heartbeats will change the status to "Yellow" and missing several will change the status to "Red". CrossStitch might be configured to start sending alerts when a stitch gets to Yellow or Red status, which can turn into phone calls and then you have IT guys getting dragged out of bed at 3AM to diagnose and fix an unhealthy system. For the sake of the IT guys, please take heartbeats seriously.

By default, `StitchMessageManager` will immediately respond to the heartbeat in the `GetNextMessage()` method and return `null` to the caller. This way, so long as you are calling `GetNextMessage()` with some regularity, you will automatically be reporting health information to the Core.

If you would like to receive the heartbeat messages and handle them yourself, such as integrating them into a larger health-monitoring system or using them as a trigger for periodic scheduled tasks (the timing on heartbeats is not precise, but if you have very course-grained scheduling needs, they might be sufficient), or if you would like to detect the absence of heartbeats as a way to detect when the Core has exited or is in distress, you can do that like this:

    _manager.ReceiveHeartbeats = true;
    var msg = _manager.GetNextMessage();
    if (msg.IsHeartbeatMessage())
    {
        // Do logic here
        _manager.SendSync(msg.Id);
    }
    
It is important that the heartbeat ID is what is passed to the `SendSync()` message. CrossStitch Core will keep track of which heartbeats are received and missed, and what the latency is between when a heartbeat goes out and the response is received. 

If you don't have a particular special need, it will be much less headache for you to let the `StitchMessageManager` handle the heartbeats by itself.

## Logging

CrossStitch has a logging mechanism that stitches can use so that they don't all have to implement their own logs. To log information, you can use either of these mechanisms:

    var logMessage = FromStitchMessage.LogMessage(new string[] {
        ... messages here ...
    });
    _manager.Send(logMessage);

or:

    _manager.SendLogs(new string[] {
        ... messages here ...
    });

This is a simple mechanism and not useful for scenarios that require structured log data or high-volume logging. The benefit to this mechanism is that logs will all be in one place, and will be available through all the logging channels that CrossStitch is configured to use.

## Sending Data Messages to Other Stitches

Stitches are grouped based on application, component and version. These values are passed in to the stitch on the command line when the process is started and are parsed by the `StitchMessageManager`. You can send a message to all stitches in a group by group name. You can also send a message to an individual Stitch by StitchID if you have that information. In either case the recipients may be locally running on the same node or remote in one of the other nodes of the CrossStitch cluster.

When a Stitch receives a data message, it is expected to respond with an Acknowledgement or Failure. If no response is received, CrossStitch may attempt to send the message again (if message queueing is implemented and enabled) or may generate alerts about the poor health of the Stitch. If a Failure response is received, that may generate alerts, error logs, or responses to the sender of problems. When you receive a data message, always confirm it by calling one of these two methods:

    _manager.AckMessage(msg.Id);
    _manager.FailMessage(msg.Id);

The data message mechanism is not optimized for throughput, reliability or latency like some other communication mechanisms may be. The goal behind this mechanism is the sharing of basic configuration, coordination, and metadata between stitches in the same group. A perfect and intended use of this mechanism would be to share communication details so that all Stitches in the group can connect to an external data store or communication channel, or to allocate access to limited resources to Stitches in the group.

To send a message, generate a `FromStitchMessage` with the required fields and send it:

    _manager.Send(message);

Send a message to a particular Stitch, if you know the ID:

    var message = FromStitchMessage.ToStitchData(stitchId, data);

Send a message to a particular Stitch, by replying to a message from that Stitch:
    
    var message = FromStitchMessage.Respond(msg, data);

To send a message to a group of Stitches, by group name:

    var message = FromStitchMessage.ToGroupData(groupName, data);

Getting information about the current group of a Stitch comes from the commandline arguments and can currently be accessed in the following way:

    // All stitches in the Application
    groupName = _manager.ApplicationGroupName;

    // All stitches in the application component
    groupName = _manager.ComponentGroupName;

    // All stitches of the same version as this application component
    groupName = _manager.VersionGroupName;

## Threading

`StitchMessageManager` uses threads internally to allow communication operations to be non-blocking and to allow timeouts. It also allows thread safety, by doing stream reads and message parsing on a dedicated reader thread, so that multiple worker threads are not all fighting over the same stream and getting incomplete/torn messages.

This is an implementation detail and not a requirement of the protocol. While this should not matter in most scenarios, certain situations where the number of running threads need to be tightly controlled may be impacted.
