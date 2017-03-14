## Logging Module

The Logging module subscribes to log messages from the system and persists or displays those to the user as necessary.

The `LoggingModule` uses an `ILog` from the `Common.Logging` project to send messages to. This is fully configurable by the hosting application and can point to any required implementation. See the `Common.Logging` project and documentation for more details about creating and configuring a log.

### Extending and Overriding

There are two options for changing the behavior of the Logging module

1. Create a new `IModule` with Name="Log", which subscribes to the `LogEvent` messages and performs the necessary logging
2. Create a new `Common.Logging.ILog` (recommended) to take messages from the LoggingModule and report them in the correct venue. See `Common.Logging` project documentation for details on using other existing or custom `ILog` implementations