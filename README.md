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
- SqlServerEventPersistor: using a single append-only table on SQL Server;
- PostgreSqlEventPersistor: **planned**;

## Building it locally

Just clone this repo and run the following commands available the Makefile, or
use them as reference to run the commands manually.

- `make startdb`: start docker instances for database tests;
- `make build`: build the library;
- `make test`: run tests and if success, generate the coverage report;
