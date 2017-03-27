# Stitches

A stitch is a user-defined chunk of code which usually runs as a separate process. CrossStitch executes and manages it, but with strict encapsulation boundaries. Neither the CrossStitch application (called the "Core") nor the Stitch process have a hard dependency on the other, and can be developed completely in isolation.

## Adaptors

CrossStitch Core interacts with a Stitch through an adaptor. There are currently two types of adaptor:

1. **ProcessV1** adaptor hosts Stitches as a separate process.
2. **BuiltInClassV1** adaptor hosts Stitches as a class compiled into the CrossStitch application itself.

The **BuiltInClassV1** adaptor is very easy for development and testing, and works well in a very small number of production applications, but is far less flexible, manageable and maintainable in the long run. Unless there is a very specific need for it, the preferred adaptor to use is the **ProcessV1** adaptor.

`CrossStitch.Stitch.dll` is the C# reference library for creating Stitch programs in C# to host under CrossStitch. The protocol for creating a Stitch is relatively simple and straight-forward, and this library is **not required** to create a functioning Stitch. Because the Stitch protocol is based only on OS-level features (command-line arguments and STDIN/STDOUT streams), you can create your own interface or use a different programming language entirely with no side effects or repercussions. If you are developing in C# you can choose `CrossStitch.Stitch.dll` to get off to a quick start. Protocol libraries for other languages may be developed in the future. Contracts, interfaces and reference implementations for all adaptor types are available in `CrossStitch.Stitch.dll`.

### ProcessV1

Each Stitch is a simple console application which implements a simple readline-based interface. Stitches read JSON-formatted messages from STDIN and write messages to STDOUT. So long as you can implement this simple protocol, you can implement your stitch in any language, or using any toolset you desire. There's no special library or dependency, no platform-dependent communication channels, and there's no favoritism. You can write a Stitch in any language you choose, so long as the server where CrossStitch is running is able to execute it.

Because Stitches are Console applications, they are very easy to develop and test. There's no magic to it: Read messages from STDIN and write messages to STDOUT, and do whatever processing you need to do in between. 

[Full adaptor documentation](adaptorprocessv1.md)

### BuiltInClassV1

The **BuiltInClassV1** adaptor is what the name suggests: A way to build a Stitch definition directly into the CrossStitch Core application. The stitch will run in the same process as the core, and will consume the same heap and use the same threadpool as the core. There are two scenarios where you may want to consider using this:

1. You are using `CrossStitch.Core` as a glue for a loosely-coupled application which will run as a single process and does not need the flexibility or scalability features of CrossStitch.
2. You have a piece of core functionality so fundamental and important that it needs to be built into the service directly and included with all deployments.
3. You are in a unit-testing scenario and it is easier for logistical reasons to to produce a stitch which is part of the test process.

In either case, please keep these caveats in mind:

1. You cannot deploy a built-in stitch separate from the core. If you need to upgrade the stitch, you will need to stop, deploy, and restart the entire service
2. Your stitch shares resources with the rest of the core including heap, threadpool, locks and others.
3. Your stitch will need to follow rules about thread safety and resource management, or risk destabilizing the entire core
4. If there is instability, your stitch could end up crashing the entire core in some situations.

There are many reasons why we want this Stitch adaptor type to be available, but there are many reasons why it probably shouldn't be used except in very specific circumstances. If you're new to CrossStitch and don't have a serious and well-considered need for this, please use **ProcessV1** instead.

## IDs Names and Groups

Stitches come with several pieces of identifying information. Every stitch has an ID, which will be unique per node and is restricted in what characters are available (depending on the particular Data Module configured in the core). Stitches have a friendly "Name" which is included in most communications, but which does not need to be provided and does not need to be unique. Stitches also belong to a **Group**.

Once you upload a Stitch package to CrossStitch, you can spin up any number of instances of that package. You can create copies on multiple nodes in the cluster, or create multiple copies on a single node, or both. If you have 5 nodes in your cluster, but want to start 10 copies of a single stitch package in the cluster. CrossStitch will likely deploy 2 instances per node. All these instances will be in the same Group, but will each potentially have different IDs.

### IDs

Ids are unique on the node, but are not necessarily unique across a cluster of nodes. CrossStitch will attempt to use a modified version of the Name to create a unique ID, and if that fails to be unique on the node, it will fall back to something like a Guid.

There are consequences of this, which may be good or bad depending on your particular needs:

1. Every node can have a local copy of an important stitch with a hard-coded ID, such as for resource location or allocation. Newly deployed stitches can send a message to the "ResourceLocator" stitch ID and get back the necessary data, without having to figure out the ID first.
2. If you want to identify a unique instance on the cluster, you will typically need to work with a "full name" of node ID and stitch Id. 

The reason why we allow non-unique IDs is due to the default cluster implementation. CrossStitch is leaderless by default, so there is no single source of truth in a cluster and no good way to make globally unique IDs without always going to something ugly like Guids. Allowing non-unique IDs allows CrossStitch to remain leaderless and also keep complexity to a minimum. Also it allows us some flexibility to use relatively "friendly" IDs where possible.

### Groups

Let's consider an example ecommerce application. Our application, "MyEcommSystem" has three components:

* "Emails" which is in charge of maintaining user email addresses and sending emails
* "OrderProcessing" which processes orders, including sending the order to the fullfillment system and sending email notifications to the user about order status
* "Specials" which keeps track of current coupons and special deals, including sending marketing emails to the users.

All these processes are in the "MyEcommSystem" application group. Under this umbrella, there are three component groups:

* "MyEcommSystem.Emails"
* "MyEcommSystem.OrderProcessing"
* "MyEcommSystem.Specials"

Every time we upload a new version of a Stitch, it creates a new Version group as well. Consider that we are migrating from an old fullfillment system to a new fullfillment system, and this requires a new version of the "OrderProcessing" component. We may end up with two groups of this component operating at the same time:

1. "MyEcommSystem.OrderProcessing.1"
2. "MyEcommSystem.OrderProcessing.2"

We can refer to a set of stitches by any of these group names: "application", "application.component" or "application.component.version". Using something like the HTTP API, we can query an entire group or send commands to an entire group at once. If we need to take the entire system down for maintenance, such as updating a shared DB server, we can send the following command:

    STOP "MyEcommSystem"
    
This command will stop all stitches in all versions of all components of the MyEcommSystem at once. 

Continuing our example of the updated fullfillment system, when we're ready to take the old system offline and send 100% of our traffic through the new system, we can issue this kind of command:

    STOP "MyEcommSystem.OrderProcessing.1"
    
This command will stop the old version of the OrderProcessing Stitches without touching the newer version or Stitches under any other component.

