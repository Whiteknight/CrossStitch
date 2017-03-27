---
layout: default
title: CrossStitch
---

# CrossStitch

## Introduction

CrossStitch is an app fabric for the .NET ecosystem. Think about something like ElasticSearch minus the search, or Kubernetes without Docker. It's a lot like Apache Storm, without the dependency on Java or the focus on big data. CrossStitch can still do processing on data streams, especially if you combine it with a data source like RabbitMQ or Kafka, but it has a lot of other uses as well.

CrossStitch is an application host with a specific focus on microservices, which are called "**Stitches**". A node can host many Stitches, including multiple instances of the same Stitch. It's sort of like a Windows Service with less ceremony, simpler administration, faster development and easier testability. Or maybe is it sort of like IIS, but instead of hosting web applications, it hosts headless processing applications.

With this simplicity comes power. You can offload expensive background processing steps from an overloaded webserver to a cluster of CrossStitch nodes. You can decompose a large, unweildy monolithic application into a suite of small and simple microservices. You can transform a collection of outdated commodity server machines into a powerful and flexible computation cluster.

## Contents

* [Contributing](contributing.md)
* [Stitch Development](stitches.md)
    * Adaptors
        * [ProcessV1 Protocol](adaptorprocessv1.md)
* [CrossStitch Core Development](core.md)
    * Built-In Modules
        * [Stitches](modulestitches.md)
        * [Master](modulemaster.md)
        * [Data](moduledata.md)
        * [Logging](modulelogging.md)
        * [Alerts](modulealerts.md)
        * [Timer](moduletimer.md)
    * Plugin Modules
      * [HTTP API](modulenancyfxapi.md) (With NancyFX)
      * [Backplane](modulezyrebackplane.md) (With NetMQ Zyre)
* [Frequently Asked Questions](faq.md)

## Installation

## Stitch Development Quickstart

See the [ProcessV1](adaptorprocessv1.md) documentation for a quickstart guide to developing your own Stitch applications.

## Etymology

[Cross stitching](https://en.wikipedia.org/wiki/Cross-stitch) is a traditional artform with colored thread and needle-work where individual stitches are made with colored thread, typically in a simple X pattern. The techniques are very simple and easy to learn, but by combining these simple stitch techniques with color and pattern, beautiful works of art can be created. In the same way, CrossStitch allows simple, easy-to-learn building blocks to be combined and repeated together to make large, complex applications.
