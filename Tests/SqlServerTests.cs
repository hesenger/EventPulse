using EventPulse;

namespace Tests;

[Collection("SqlServerCollection")]
public class SqlServerTests
{
    private SqlServerFixture _fixture;

    public SqlServerTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    private EventStore CreateDefaultEventStore()
    {
        var provider = new ListAggregatorProvider();
        provider.Register(new PersonEventAggregator());

        return new EventStore(
            new SqlServerEventPersistor(_fixture.DatabaseProvider, new JsonEventSerializer()),
            provider
        );
    }

    [Fact]
    public async Task RetrievesAggregateRootFullyRestored()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(new PersonCreatedEvent("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.Equal("John Doe", restoredPerson!.Name);
    }
}
