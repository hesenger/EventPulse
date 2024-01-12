using System.Data;
using EventPulse;
using Microsoft.Data.SqlClient;

namespace Tests;

public class SqlServerConnectionProvider : IConnectionProvider
{
    private readonly string _connectionString;

    public SqlServerConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection GetConnection()
    {
        var cnx = new SqlConnection(_connectionString);
        return cnx;
    }
}
