## Logging Module

The Logging module subscribes to log messages from the system and persists or displays those to the user as necessary.

The `LoggingModule` uses an `ILog` from the `Common.Logging` project to send messages to. This is fully configurable by the hosting application and can point to any required implementation. See the `Common.Logging` project and documentation for more details about creating and configuring a log.