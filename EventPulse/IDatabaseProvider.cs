using System.Data;

namespace EventPulse;

public interface IDatabaseProvider
{
    IDbConnection GetConnection();
}
