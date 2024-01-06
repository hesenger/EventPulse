using System.Data;

namespace EventPulse;

public interface IDatabaseProvider
{
    IDbCommand GetCommand();
}
