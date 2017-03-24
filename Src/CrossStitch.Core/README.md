# CrossStitch.Core

This is the core library of the CrossStitch system. It implements all core behaviors, though most of those behaviors are pluggable and able to be overridden by custom implementations and extensions.

The CrossSystem system is designed to be modular and flexible for an unforseeably large set of use-cases. Modules are loosely coupled and communicate with each other via a message-passing system. Some abilities, such as the ability to coordinate CrossStitch instances into a cluster, to host an HTTP API, to host Stitches, and others are completely optional and may be turned on or off depending on use-case.

## QuickStart

For an extremely simple node, create a `CrossStitchCore` object  and call the `.Start()` method. 

    using (var node = new CrossStitchCore(config))
    {
        node.Start();
        // Do something here to determine when to stop, such as waiting on a
        // ManualResetEvent
        Console.ReadKey();
        node.Stop();
    }

This node won't do anything by default, so you will probably want to put in a few modules to add new behaviors.

    using (var node = new CrossStitchCore(config))
    {
        // Allow to run stitch processes
        var stitches = new StitchesModule(StitchesConfiguration.GetDefault());
        node.AddModule(stitches);

        // Defined in CrossStitch.Backplane.Zyre.dll, allow the node to assemble
        // into a cluster with other nodes
        core.AddModule(new BackplaneModule());

        // Defined in CrossStitch.Http.NancyFx.dll, the node will host an HTTP
        // API for interacting with the node
        var httpConfiguration = HttpConfiguration.GetDefault();
        var httpServer = new NancyHttpModule(httpConfiguration, core.MessageBus);
        core.AddModule(httpServer);

        // Enable logging. Configure Common.Logging separately.
        var log = Common.Logging.LogManager.GetLogger("CrossStitch");
        var logging = new LoggingModule(log);
        node.AddModule(logging);

        node.Start();
        // Do something here to determine when to stop, such as waiting on a
        // ManualResetEvent
        Console.ReadKey();
        node.Stop();
    }

Now, when you compile and run this program, it will have almost all features of CrossStitch including hosting stitches, automatic clustering, and a full-featured RESTful API for external interactions. Depending on your desired cluster topology, you can use some or all of these modules to get the desired behaviors.

## Further Reading

See the readme files under the Modules directory for more details about each specific module, what the module does, and how to override it.