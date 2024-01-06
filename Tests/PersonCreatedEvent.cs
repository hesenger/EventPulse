namespace Tests;

public record PersonCreatedEvent(Guid Id, string Name, DateTime CreatedAt)
{
    public PersonCreatedEvent(string name)
        : this(Guid.NewGuid(), name, DateTime.UtcNow) { }
}
