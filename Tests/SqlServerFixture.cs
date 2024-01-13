using EventPulse;

namespace Tests;

public class SqlServerFixture
{
    public readonly IConnectionProvider ConnectionProvider;

    public SqlServerFixture()
    {
        var connectionString =
            "Server=localhost;Database=master;UID=sa;PWD=DevPassword-2024;Connect Timeout=5;TrustServerCertificate=True;";
        ConnectionProvider = new SqlServerConnectionProvider(connectionString);

        DropAndCreateTable();
    }

    private void DropAndCreateTable()
    {
        using var connection = ConnectionProvider.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "IF OBJECT_ID('Events', 'U') IS NOT NULL DROP TABLE Events";
        command.ExecuteNonQuery();

        command.CommandText =
            @"
            CREATE TABLE Events
            (
                Id INT PRIMARY KEY IDENTITY(1,1),
                StreamName NVARCHAR(100) NOT NULL,
                StreamId BIGINT NOT NULL,
                Revision INT NOT NULL,
                EventType NVARCHAR(100) NOT NULL,
                EventData NVARCHAR(MAX) NOT NULL,
                Timestamp DATETIME2 NOT NULL,
                INDEX IX_StreamRevision UNIQUE (StreamName, StreamId, Revision)
            )";
        command.ExecuteNonQuery();
    }
}
