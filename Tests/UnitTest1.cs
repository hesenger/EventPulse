using NSubstitute;

namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore(IEventPersistor? persistor = null)
    {
        return new EventStore(persistor ?? Substitute.For<IEventPersistor>());
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
}

public interface IEventPersistor
{
    Task Persist(object aggregateRootId, object evt);
}

public class EventStore
{
    private readonly IEventPersistor _eventPersistor;

    public EventStore(IEventPersistor eventPersistor)
    {
        _eventPersistor = eventPersistor;
    }

    private object _obj = null!;

    public async Task<TAggregateRoot?> Find<TAggregateRoot>(object id)
    {
        await Task.CompletedTask;
        return (TAggregateRoot?)_obj;
    }

    public async Task Save<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : IStateHolder
    {
        await Task.CompletedTask;
        var (id, events) = aggregateRoot.GetState();
        events.ToList().ForEach(evt => _eventPersistor.Persist(id, evt));
        _obj = aggregateRoot;
    }
}

public interface IStateHolder
{
    (object AggregateRootId, IReadOnlyList<object> Events) GetState();
}

public class Person : IStateHolder
{
    private readonly List<object> _events = new();

    public Guid Id { get; }
    public string Name { get; private set; }
    private readonly DateTime _createdAt;

    public Person(PersonCreatedEvent evt)
    {
        Id = evt.Id;
        Name = evt.Name;
        _createdAt = evt.CreatedAt;
        _events.Add(evt);
    }

    public void UpdateName(PersonNameUpdatedEvent evt)
    {
        Name = evt.Name;
        _events.Add(evt);
    }

    public (object, IReadOnlyList<object>) GetState() => (Id, _events);
}

public record PersonNameUpdatedEvent(string Name);

public record PersonCreatedEvent(Guid Id, string Name, DateTime CreatedAt)
{
    public PersonCreatedEvent(string name)
        : this(Guid.NewGuid(), name, DateTime.UtcNow)
    {
    }
}
