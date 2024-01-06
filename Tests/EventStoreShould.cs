using EventPulse;
using NSubstitute;

namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore(IEventPersistor? persistor = null)
    {
        var provider = new ListAggregatorProvider();
        provider.Register<Person>(new PersonEventAggregator());

        return new EventStore(persistor ?? Substitute.For<IEventPersistor>(), provider);
    }

    [Fact]
    public async Task RetrieveAggregateRootFullyRestored()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(new PersonCreatedEvent("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.Equal("John Doe", restoredPerson!.Name);
    }

    [Fact]
    public async Task PersistEventsInPersistor()
    {
        var eventPersistor = Substitute.For<IEventPersistor>();
        var eventStore = CreateDefaultEventStore(eventPersistor);
        var person = new Person(new PersonCreatedEvent("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonCreatedEvent>());
        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonNameUpdatedEvent>());
    }

    [Fact]
    public async Task PersonRetrievedIsNewInstance()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(new PersonCreatedEvent("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.NotSame(person, restoredPerson);
    }
}
