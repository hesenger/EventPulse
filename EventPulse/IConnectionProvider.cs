using System.Data;

namespace EventPulse;

public interface IConnectionProvider
{
    IDbConnection GetConnection();
}
