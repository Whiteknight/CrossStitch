# CrossStitch

## Introduction

CrossStitch is an app fabric for the .NET ecosystem. Think about something like ElasticSearch minus the search, or Kubernetes without Docker. It's a lot like Apache Storm, without the dependency on Java or the focus on big data. CrossStitch can still do processing on data streams, especially if you combine it with a data source like RabbitMQ or Kafka, but it has a lot of other uses as well.

CrossStitch is an application host with a specific focus on microservices, which are called "Stitches". A node can host many Stitches, including multiple instances of the same Stitch. It's sort of like a Windows Service with less ceremony, simpler administration, faster development and easier testability.

## Design Goals

Cross stitching is a traditional artform with colored thread and needle-work where individual stitches are made with colored thread, typically in a simple X pattern. The techniques are very simple and easy to learn. By combining these simple stitches with color and pattern, beautiful works of art can be created. In the same way, CrossStitch allows simple, easy-to-learn building blocks to be combined and repeated together to make large, complex applications.

Creating, deploying, monitoring and managing stitches should be fast and easy. Developers shouldn't need special tooling, specific languages, and deep knowledge about the workings of CrossStitch to get things working. If you can write a console application, you can host applications on CrossStitch.

The CrossStitch core should be modular and loosely-coupled, following modern best practices and modern idiomatic C#.

CrossStitch should provide sane but simple defaults, while allowing users to write and substitute their own, more complex versions as required.

## Stitches

 Each Stitch is a simple console application which implements a simple readline-based interface. Stitches read JSON-formatted messages from STDIN and write messages to STDOUT. There's no special library or dependency, no platform-dependent communication channels, and there's no favoritism. You can write a Stitch in any language you choose, so long as the server where CrossStitch is running is able to execute it.

 Because Stitches are Console applications, they are very easy to develop and test. There's no magic to it: Read messages from STDIN and write messages to STDOUT, and do whatever processing you need to do in between. 

 With the simplicity comes power. You can offload expensive background processing steps from an overloaded webserver to a cluster of CrossStitch nodes. You can decompose a large, unweildy monolithic application into a suite of small and simple microservices. You can transform a collection of outdated commodity server machines into a powerful and flexible computation cluster

## Architecture 

The heart of CrossStitch is CrossStitch.Core.dll, which implements the core logic and infrastructure. CrossStitch is modular by design, and different combinations and configurations of modules can lead to different behaviors.

With a backplane module installed, CrossStitch instances from across a network can self-assemble into a cluster, and allow coordinated operations and communication.

With an HTTP module installed, CrossStitch instances will expose an HTTP API which can be used to query state and issue commands to the node

A LoggingModule allows you to get and persist debug logs from both the core process and the running Stitches.

An alerting module allows CrossStitch to push alerts to your IT team when things are going wrong or when Stitches are becoming unresponsive.

## Usage

With a few lines of code and some configuration, you can write your own CrossStitch-enabled application in C# or any .NET language. There will also be some out-of-the-box solutions available, if you are looking for an executable with standard options.
