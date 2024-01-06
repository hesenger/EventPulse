using System.Data;

namespace EventPulse;

public class SqlServerEventPersistor : IEventPersistor
{
    private readonly IDatabaseProvider _database;
    private readonly IEventSerializer _serializer;

    public SqlServerEventPersistor(IDatabaseProvider databaseProvider, IEventSerializer serializer)
    {
        _database = databaseProvider;
        _serializer = serializer;
    }

    public async Task Persist(object aggregationId, object evt)
    {
        await Task.CompletedTask;

        using var cnx = _database.GetConnection();
        using var command = cnx.CreateCommand();
        command.CommandText =
            "INSERT INTO Events (AggregationId, EventType, EventData, Timestamp) "
            + "VALUES (@AggregationId, @EventType, @EventData, @Timestamp)";
        AddParameter(command, "@AggregationId", aggregationId);
        AddParameter(command, "@EventType", evt.GetType().FullName!);
        AddParameter(command, "@EventData", _serializer.Serialize(evt));
        AddParameter(command, "@Timestamp", DateTime.UtcNow);

        command.ExecuteNonQuery();
    }

    public async Task<IEnumerable<object>> GetEvents(object aggregationId)
    {
        await Task.CompletedTask;

        using var cnx = _database.GetConnection();
        using var command = cnx.CreateCommand();
        command.CommandText =
            "SELECT EventType, EventData FROM Events WHERE AggregationId = @AggregationId ORDER BY Timestamp ASC";
        AddParameter(command, "@AggregationId", aggregationId);

        using var reader = command.ExecuteReader();
        var events = new List<object>();

        while (reader.Read())
        {
            var eventType = reader["EventType"].ToString()!;
            var eventData = reader["EventData"].ToString()!;
            events.Add(_serializer.Deserialize(eventType, eventData));
        }

        return events;
    }

    private void AddParameter(IDbCommand command, string parameterName, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
