namespace EventPulse;

public interface IEventPersistor
{
    Task Persist(object aggregationId, object evt);

    Task<IEnumerable<object>> GetEvents(object aggregationId);
}
