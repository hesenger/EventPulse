namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore()
    {
        return new EventStore();
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
}

public class EventStore
{
    public async Task<TAggregateRoot?> Find<TAggregateRoot>(object id)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task Save(Person person)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}

public class Person
{
    public Guid Id { get; }
    public string Name { get; private set; }
    private readonly DateTime _createdAt;

    public Person(PersonCreatedEvent evt)
    {
        Id = evt.Id;
        Name = evt.Name;
        _createdAt = evt.CreatedAt;
    }

    public void UpdateName(PersonNameUpdatedEvent evt)
    {
        Name = evt.Name;
    }
}

public record PersonNameUpdatedEvent(string Name);

public record PersonCreatedEvent(Guid Id, string Name, DateTime CreatedAt)
{
    public PersonCreatedEvent(string name)
        : this(Guid.NewGuid(), name, DateTime.UtcNow)
    {
    }
}
