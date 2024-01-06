using EventPulse;

namespace Tests;

public class SqlServerFixture
{
    public readonly IDatabaseProvider DatabaseProvider;

    public SqlServerFixture()
    {
        var connectionString =
            "Server=localhost;Database=master;UID=sa;PWD=DevPassword-2024-01-06;Connect Timeout=5;TrustServerCertificate=True;";
        DatabaseProvider = new SqlServerDatabaseProvider(connectionString);

        DropAndCreateTable();
    }

    private void DropAndCreateTable()
    {
        using var connection = DatabaseProvider.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "IF OBJECT_ID('Events', 'U') IS NOT NULL DROP TABLE Events";
        command.ExecuteNonQuery();

        command.CommandText =
            @"
            CREATE TABLE Events
            (
                Id INT PRIMARY KEY IDENTITY(1,1),
                AggregationId NVARCHAR(100) NOT NULL,
                EventType NVARCHAR(100) NOT NULL,
                EventData NVARCHAR(MAX) NOT NULL,
                Timestamp DATETIME2 NOT NULL
                INDEX IX_AggregationId NONCLUSTERED (AggregationId)
            )";
        command.ExecuteNonQuery();
    }
}
