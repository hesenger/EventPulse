namespace Tests;

public record PersonCreatedEvent(Guid Id, string Name, DateTime CreatedAt)
{
    public static PersonCreatedEvent Create(string name) =>
        new(Guid.NewGuid(), name, DateTime.UtcNow);
}
