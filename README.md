# CrossStitch

## Introduction

CrossStitch is an app fabric for the .NET ecosystem. Think about something like ElasticSearch minus the search, or
Kubernetes without Docker. 

Install CrossStitch on servers with free resources, and they will self-assemble into a cluster. You can deploy
headless applications to CrossStitch, and those applications will automatically be deployed to nodes in the
cluster. 

CrossStitch applications will run on the CrossStitch cluster by themselves. It's sort of like a Windows Service with
less ceremony, simpler administration, easier development and easier testability. CrossStitch applications can be any
DLL or executable, and only need to define an entrypoint.

Consider the example of system built from a number of services or microservices. Services read messages on some message
channel (subscribe to a queue on RabbitMQ or a topic in Kafka, etc) and perform some calculations or transformations
on those messages. CrossStitch can be used to host these applications, and to easily add new instances in response
to demand.

## Architecture

CrossStitch is a modular, loosly-coupled application. Each node is configurable, and can perform different tasks
as needed.