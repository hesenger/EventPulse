# EventPulse

EventPulse provides a thin layer between your application and the event persistence
for Event Sourcing systems. It's designed to be less intrusive as possible and
avoid totally reflection, which means the real methods are used to retrieve the
aggregations based in the events stored previously.

## Code

The library was written since the initial commit using TDD, so code coverage is not
a problem, furthermore, you can check the specs written to quickly undestand the
usage and advanced features.

## Persistence support

The events can be stored using the following implementations:

- InMemoryEventPersistor: useful for testing purposes;
- SqlServerEventPersistor: using a single append only table on SQL Server;
- PostgreSqlEventPersistor: **planned**;
