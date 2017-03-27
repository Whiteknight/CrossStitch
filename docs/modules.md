# Modules

Internally, CrossStitch is broken up into several separate modules, which are independent and loosely coupled. Modules do not interact with one another directly, but instead communicate indirectly over the message bus.

## Design Considerations

All modules implement the `IModule` interface. A good way to think about modules is that they are an implementation of the adaptor pattern between the message bus and the individual subdomain. For example, the class `DataModule` sets up subscriptions on the bus and passes those requests over to the `DataService` where the subdomain logic is implemented. Keeping the message bus wiring separate from the subdomain logic helps with testability and other concerns.