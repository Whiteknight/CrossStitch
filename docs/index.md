# CrossStitch

## Introduction

CrossStitch is an app fabric for the .NET ecosystem. Think about something like ElasticSearch minus the search, or Kubernetes without Docker. It's a lot like Apache Storm, without the dependency on Java or the focus on big data. CrossStitch can still do processing on data streams, especially if you combine it with a data source like RabbitMQ or Kafka, but it has a lot of other uses as well.

CrossStitch is an application host with a specific focus on microservices, which are called "Stitches". A node can host many Stitches, including multiple instances of the same Stitch. It's sort of like a Windows Service with less ceremony, simpler administration, faster development and easier testability. Or maybe is it sort of like IIS, but instead of hosting web applications, it hosts headless processing applications.

With the simplicity comes power. You can offload expensive background processing steps from an overloaded webserver to a cluster of CrossStitch nodes. You can decompose a large, unweildy monolithic application into a suite of small and simple microservices. You can transform a collection of outdated commodity server machines into a powerful and flexible computation cluster.

* [Contributing](contributing.md)
* [Stitches](stitches.md)
    * Adaptors
        * [ProcessV1](adaptorprocessv1.md)s
* [Core](core.md)
    * Modules
        * Stitches
        * [Master](modulemaster.md)
        * [Data](moduledata.md)
