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

Custom implementations can be easily made by implementing `IEventPersistor`.

## Building it locally

Just clone this repo and run the following commands available the Makefile, or
use them as reference to run the commands manually.

- `make startdb`: start docker instances for database tests;
- `make build`: build the library;
- `make test`: run tests and if success, generate the coverage report;
- `make stopdb`: stop docker instances;

## Usage

For web apps you likely want to register the EventStore instance in your
service provider, but in fact there is no harm in using multiple instances or
even a simple singlton since the actions are very atomic and not state dependant.

```csharp
var provider = new ListAggregatorProvider();
provider.Register(new PersonEventAggregator());
var eventStore = new EventStore(new InMemoryEventPersistor(), provider);

var person = new Person(new PersonCreatedEvent("John"));
person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
await eventStore.Save(person);

var restoredPerson = await eventStore.Find<Person>(person.Id);
Assert.Equal("John Doe", restoredPerson!.Name);
```
