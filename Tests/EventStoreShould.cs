using System.Transactions;
using EventPulse;
using NSubstitute;

namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore(IEventPersistor? persistor = null)
    {
        var provider = new ListAggregatorProvider();
        provider.Register(new PersonEventAggregator());

        return new EventStore(persistor ?? new InMemoryEventPersistor(), provider);
    }

    [Fact]
    public async Task RetrieveAggregateRootFullyRestored()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.Equal("John Doe", restoredPerson!.Name);
    }

    [Fact]
    public async Task PersistEventsInPersistor()
    {
        var eventPersistor = Substitute.ForPartsOf<InMemoryEventPersistor>();
        var eventStore = CreateDefaultEventStore(eventPersistor);
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonCreatedEvent>());
        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonNameUpdatedEvent>());
    }

    [Fact]
    public async Task RetrieveNewInstanteOfPerson()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.NotNull(restoredPerson);
        Assert.NotSame(person, restoredPerson);
    }

    [Fact]
    public async Task RetrieveNullWhenAggregateRootNotFound()
    {
        var eventStore = CreateDefaultEventStore();
        var restoredPerson = await eventStore.Find<Person>(Guid.NewGuid());
        Assert.Null(restoredPerson);
    }
}
