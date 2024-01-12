using System.Data;
using EventPulse;
using Microsoft.Data.SqlClient;

namespace Tests;

public class SqlServerDatabaseProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public SqlServerDatabaseProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection GetConnection()
    {
        var cnx = new SqlConnection(_connectionString);
        return cnx;
    }
}
