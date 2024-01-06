using EventPulse;

namespace Tests;

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
