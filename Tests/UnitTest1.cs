using NSubstitute;

namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore(IEventPersistor? persistor = null)
    {
        var provider = new ListAggregatorProvider();
        provider.Register<Person>(new PersonEventAggregator());

        return new EventStore(
            persistor ?? Substitute.For<IEventPersistor>(),
            provider);
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

public class ListAggregatorProvider : IEventAggregatorProvider
{
    private readonly Dictionary<Type, object> _aggregators = new();

    public void Register<TAggregation>(IEventAggregator<TAggregation> aggregator)
    {
        _aggregators.Add(typeof(TAggregation), aggregator);
    }

    public IEventAggregator<TAggregation> GetAggregator<TAggregation>()
    {
        throw new NotImplementedException();
    }
}

public interface IEventPersistor
{
    Task Persist(object stateHolderId, object evt);

    Task<IEnumerable<object>> GetEvents(object stateHolderId);
}

public interface IEventAggregatorProvider
{
    IEventAggregator<TAggregation> GetAggregator<TAggregation>();
}

public interface IEventAggregator<TAggregation>
{
    TAggregation Aggregate(TAggregation? aggregation, object evt);
}

public class EventStore
{
    private readonly IEventPersistor _persistor;
    private readonly IEventAggregatorProvider _aggregatorProvider;

    public EventStore(IEventPersistor eventPersistor, IEventAggregatorProvider aggregatorProvider)
    {
        _persistor = eventPersistor;
        _aggregatorProvider = aggregatorProvider;
    }

    private object _obj = null!;

    public async Task<TAggregateRoot?> Find<TAggregateRoot>(object id)
    {
        var events = await _persistor.GetEvents(id);
        var aggregator = _aggregatorProvider.GetAggregator<TAggregateRoot>();
        var aggregation = events.Aggregate(default(TAggregateRoot?), aggregator.Aggregate);
        return aggregation;
    }

    public async Task Save<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : IAggregation
    {
        await Task.CompletedTask;
        var (id, events) = aggregateRoot.GetState();
        events.ToList().ForEach(evt => _persistor.Persist(id, evt));
        _obj = aggregateRoot;
    }
}

public interface IAggregation
{
    (object AggregationId, IReadOnlyList<object> Events) GetState();
}

public class PersonEventAggregator : IEventAggregator<Person>
{
    public Person Aggregate(Person? person, object evt)
    {
        if (evt is PersonCreatedEvent created)
            return new Person(created);

        if (evt is PersonNameUpdatedEvent updated)
            person!.UpdateName(updated);

        return person!;
    }
}

public class Person : IAggregation
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
