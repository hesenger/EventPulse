using System.Data;

namespace EventPulse;

public class SqlServerStreamPersistor : IStreamPersistor
{
    private readonly IDatabaseProvider _database;

    public SqlServerStreamPersistor(IDatabaseProvider databaseProvider)
    {
        _database = databaseProvider;
    }

    private void AddParameter(IDbCommand command, string parameterName, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    public void Persist(
        string streamName,
        object streamId,
        int revision,
        string eventType,
        string eventData
    )
    {
        using var cnx = _database.GetConnection();
        cnx.Open();

        using var command = cnx.CreateCommand();
        command.CommandText = """
            INSERT INTO Events (StreamName, StreamId, Revision, EventType, EventData, Timestamp) 
            VALUES (@StreamName, @StreamId, @Revision, @EventType, @EventData, @Timestamp)
            """;
        AddParameter(command, "@StreamName", streamName);
        AddParameter(command, "@StreamId", streamId);
        AddParameter(command, "@Revision", revision);
        AddParameter(command, "@EventType", eventType);
        AddParameter(command, "@EventData", eventData);
        AddParameter(command, "@Timestamp", DateTime.UtcNow);

        command.ExecuteNonQuery();
    }

    public IEnumerable<EventResult> GetEvents(string streamName, object streamId)
    {
        using var cnx = _database.GetConnection();
        cnx.Open();

        using var command = cnx.CreateCommand();
        command.CommandText = """
            SELECT StreamName, StreamId, Revision, EventType, EventData 
            FROM Events
            WHERE StreamName = @StreamName AND StreamId = @StreamId 
            ORDER BY Revision ASC
            """;
        AddParameter(command, "@StreamName", streamName);
        AddParameter(command, "@StreamId", streamId);

        using var reader = command.ExecuteReader();
        var events = new List<object>();

        while (reader.Read())
        {
            yield return new EventResult(
                reader.GetString(0),
                reader.GetValue(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4)
            );
        }
    }
}
