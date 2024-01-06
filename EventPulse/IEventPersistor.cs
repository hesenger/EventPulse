namespace EventPulse;

public interface IEventPersistor
{
    Task Persist(object stateHolderId, object evt);

    Task<IEnumerable<object>> GetEvents(object stateHolderId);
}
