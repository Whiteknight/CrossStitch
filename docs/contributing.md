# CrossStitch Development

## Design Goals

CrossStitch follows some of the same basic guidelines: Complexity emerges from repeating and combining large numbers of very simple building blocks. Here are some of the design goals for CrossStitch:

1. Creating, deploying, monitoring and managing stitches should be fast and easy. Developers shouldn't need special tooling, specific languages, and deep knowledge about the workings of CrossStitch to get things working. If you can write a console application in your language of choice, you can create applications for CrossStitch.
2. The CrossStitch core should be modular and loosely-coupled, following modern best practices and modern idiomatic C#. 
3. CrossStitch is a library first, an application second. Where the defaults of our provided applications are insufficient, developers should be able to easily and confidently create their own.
4. CrossStitch should provide sane but simple defaults, while allowing users to write and substitute their own, more complex versions as required.
5. CrossStitch should be asynchronous and non-blocking as much as possible. Request/Response scenarios should be minimized, preferring Pub/Sub with correlation IDs to track state where possible.

The initial release is expected to run on .NET Framework 4.6, but portability to Mono and .NET Core are on the roadmap and are highly desireable

## Architecture 

The heart of CrossStitch is `CrossStitch.Core.dll`, which implements the core logic and infrastructure. CrossStitch is modular by design, and different combinations and configurations of modules can lead to different behaviors.

With a **backplane module** installed, CrossStitch instances from across a network can self-assemble into a cluster, and allow coordinated operations and communication.

With an **HTTP module** installed, CrossStitch instances will expose an HTTP API which can be used to query state and issue commands to the node, and may even expose a UI for easy browser-based interactions.

The **stitches module** allows the node to host Stitch applications. Omitting it frees the node to perform other tasks with greater focus.

A **logging module** allows you to get and persist debug logs from both the core process and the running Stitches.

An **alerting module** allows CrossStitch to push alerts to your IT team when things are going wrong or when Stitches are becoming unresponsive.

By following best practices for loosely-coupled applications, we should be able to have a system where some or all of these modules are provided or omitted, and the system should function as expected.

## Contributing

CrossStitch is looking for good developers, including C# developers and developers for other languages and specialities. 

The easiest way to get involved is to create a Fork on Github, start making changes and additions, and then open a Pull Request.

CrossStitch is available under the **Apache 2.0 license**, and all contributions should be made with that in mind.
